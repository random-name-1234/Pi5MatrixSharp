using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Pi5MatrixSharp;

/// <summary>
/// Managed wrapper over <c>libpi5matrix.so</c> for Raspberry Pi 5 HUB75 panels.
/// Thread-safe: concurrent writes to the back buffer and <see cref="Show"/> calls
/// are synchronised internally.
/// </summary>
public sealed class Pi5Matrix : IDisposable
{
    private static readonly byte[] DefaultGammaLut = BuildGammaLut(2.5f);

    private readonly GCHandle pinnedFrontBuffer;
    private readonly byte[] frontBuffer;
    private readonly object showLock = new();
    private readonly Stopwatch showStopwatch = new();
    private IntPtr handle;
    private bool disposed;
    private float brightness = 1f;
    private bool gammaEnabled;
    private byte[] gammaLut = DefaultGammaLut;
    private long showCount;
    private double lastShowDurationMs;

    /// <summary>
    /// Creates a new matrix instance with double-buffered rendering.
    /// </summary>
    public Pi5Matrix(Pi5MatrixOptions options)
    {
        Options = options;
        ValidateGeometryOptions(options.Geometry);
        var expectedSize = checked((int)Pi5Bindings.pi5_matrix_expected_framebuffer_size(
            (int)options.Colorspace,
            options.Geometry.Width,
            options.Geometry.Height));
        if (expectedSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "The selected colorspace/geometry produced an invalid framebuffer size.");

        // Double-buffering: back buffer (user-facing) + front buffer (pinned, hardware-facing)
        FrameBuffer = new byte[expectedSize];
        frontBuffer = new byte[expectedSize];
        pinnedFrontBuffer = GCHandle.Alloc(frontBuffer, GCHandleType.Pinned);

        GCHandle pinnedPixelMap = default;
        try
        {
            var pixelMapPointer = IntPtr.Zero;
            nuint pixelMapLength = 0;
            if (options.Geometry.PixelMap is { Length: > 0 } pixelMap)
            {
                pinnedPixelMap = GCHandle.Alloc(pixelMap, GCHandleType.Pinned);
                pixelMapPointer = pinnedPixelMap.AddrOfPinnedObject();
                pixelMapLength = checked((nuint)pixelMap.Length);
            }

            var geometry = new InternalPi5MatrixGeometryOptions(
                options.Geometry,
                pixelMapPointer,
                pixelMapLength);

            handle = Pi5Bindings.pi5_matrix_create_from_buffer(
                (int)options.Colorspace,
                (int)options.Pinout,
                pinnedFrontBuffer.AddrOfPinnedObject(),
                (nuint)frontBuffer.Length,
                ref geometry);

            if (handle == IntPtr.Zero)
            {
                pinnedFrontBuffer.Free();
                throw new InvalidOperationException(
                    $"Failed to create Pi 5 matrix: {Pi5Bindings.GetLastErrorMessage()}");
            }
        }
        finally
        {
            if (pinnedPixelMap.IsAllocated)
                pinnedPixelMap.Free();
        }
    }

    /// <summary>
    /// Options used to create this matrix instance.
    /// </summary>
    public Pi5MatrixOptions Options { get; }

    /// <summary>
    /// Back buffer in the configured colorspace. Write pixels here, then call <see cref="Show"/>
    /// to push them to the hardware.
    /// </summary>
    public byte[] FrameBuffer { get; }

    /// <summary>
    /// Panel width in pixels.
    /// </summary>
    public int Width => Options.Geometry.Width;

    /// <summary>
    /// Panel height in pixels.
    /// </summary>
    public int Height => Options.Geometry.Height;

    /// <summary>
    /// Hardware refresh rate reported by the native backend.
    /// </summary>
    public double FramesPerSecond
    {
        get
        {
            ThrowIfDisposed();
            return Pi5Bindings.pi5_matrix_get_fps(handle);
        }
    }

    /// <summary>
    /// Master brightness (0.0 = off, 1.0 = full). Applied during <see cref="Show"/>
    /// when copying to the front buffer.
    /// </summary>
    public float Brightness
    {
        get => brightness;
        set => brightness = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// When true, a gamma correction curve is applied during <see cref="Show"/>.
    /// Default gamma is 2.5, suitable for most HUB75 panels.
    /// </summary>
    public bool GammaCorrection
    {
        get => gammaEnabled;
        set => gammaEnabled = value;
    }

    /// <summary>
    /// Sets the gamma exponent used when <see cref="GammaCorrection"/> is enabled.
    /// Default is 2.5. Common values for LED panels are 2.2–2.8.
    /// </summary>
    public float GammaExponent
    {
        set
        {
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(nameof(value), "Gamma exponent must be positive.");
            gammaLut = BuildGammaLut(value);
        }
    }

    /// <summary>
    /// Returns the underlying back buffer as a mutable span.
    /// </summary>
    public Span<byte> GetFrameBuffer() => FrameBuffer;

    /// <summary>
    /// Clears the back buffer to black.
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();
        Array.Clear(FrameBuffer);
    }

    /// <summary>
    /// Copies a packed RGB888 frame into the back buffer.
    /// </summary>
    public void CopyRgb24(ReadOnlySpan<byte> rgb24)
    {
        ThrowIfDisposed();
        if (Options.Colorspace != Pi5Colorspace.Rgb888Packed)
            throw new InvalidOperationException("CopyRgb24() requires Rgb888Packed colorspace.");
        if (rgb24.Length != FrameBuffer.Length)
            throw new ArgumentOutOfRangeException(nameof(rgb24));

        rgb24.CopyTo(FrameBuffer);
    }

    /// <summary>
    /// Copies an RGBA32 frame (4 bytes per pixel) into the RGB888Packed back buffer,
    /// dropping the alpha channel. This matches SixLabors.ImageSharp Rgba32 layout.
    /// </summary>
    public void CopyRgba32(ReadOnlySpan<byte> rgba32)
    {
        ThrowIfDisposed();
        if (Options.Colorspace != Pi5Colorspace.Rgb888Packed)
            throw new InvalidOperationException("CopyRgba32() requires Rgb888Packed colorspace.");

        var expectedRgba = Width * Height * 4;
        if (rgba32.Length != expectedRgba)
            throw new ArgumentOutOfRangeException(nameof(rgba32),
                $"Expected {expectedRgba} bytes (RGBA32), got {rgba32.Length}.");

        var dst = FrameBuffer;
        var pixelCount = Width * Height;
        for (var i = 0; i < pixelCount; i++)
        {
            var srcOffset = i * 4;
            var dstOffset = i * 3;
            dst[dstOffset] = rgba32[srcOffset];
            dst[dstOffset + 1] = rgba32[srcOffset + 1];
            dst[dstOffset + 2] = rgba32[srcOffset + 2];
            // alpha (srcOffset + 3) is dropped
        }
    }

    /// <summary>
    /// Sets one pixel in the RGB888Packed back buffer.
    /// </summary>
    public void SetPixel(int x, int y, byte r, byte g, byte b)
    {
        ThrowIfDisposed();
        if (Options.Colorspace != Pi5Colorspace.Rgb888Packed)
            throw new InvalidOperationException("SetPixel() requires Rgb888Packed colorspace.");
        if ((uint)x >= (uint)Width)
            throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)Height)
            throw new ArgumentOutOfRangeException(nameof(y));

        var offset = (y * Width + x) * 3;
        FrameBuffer[offset] = r;
        FrameBuffer[offset + 1] = g;
        FrameBuffer[offset + 2] = b;
    }

    /// <summary>
    /// Copies the back buffer to the pinned front buffer (applying brightness and gamma
    /// if configured), then pushes it to the matrix hardware.
    /// </summary>
    public void Show()
    {
        ThrowIfDisposed();

        lock (showLock)
        {
            showStopwatch.Restart();

            ApplyTransformsToFrontBuffer();

            var err = Pi5Bindings.pi5_matrix_show(handle);
            if (err != 0)
                throw new IOException($"Pi 5 matrix show() failed with errno {err}: {Pi5Bindings.GetLastErrorMessage()}");

            showStopwatch.Stop();
            lastShowDurationMs = showStopwatch.Elapsed.TotalMilliseconds;
            Interlocked.Increment(ref showCount);
        }
    }

    /// <summary>
    /// Returns a snapshot of diagnostic information about this matrix instance.
    /// </summary>
    public Pi5MatrixDiagnostics GetDiagnostics()
    {
        ThrowIfDisposed();
        return new Pi5MatrixDiagnostics(
            Width,
            Height,
            Options.Colorspace,
            Options.Pinout,
            FramesPerSecond,
            Interlocked.Read(ref showCount),
            lastShowDurationMs,
            FrameBuffer.Length,
            Brightness,
            GammaCorrection);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var fps = disposed ? 0 : FramesPerSecond;
        return $"Pi5Matrix {Width}x{Height} {Options.Colorspace} @ {fps:F0}fps";
    }

    private void ApplyTransformsToFrontBuffer()
    {
        var src = FrameBuffer;
        var dst = frontBuffer;
        var needsBrightness = brightness < 0.999f;
        var needsGamma = gammaEnabled;

        if (!needsBrightness && !needsGamma)
        {
            Buffer.BlockCopy(src, 0, dst, 0, src.Length);
            return;
        }

        var lut = needsGamma ? gammaLut : null;
        var bright = brightness;

        for (var i = 0; i < src.Length; i++)
        {
            var value = src[i];
            if (lut is not null)
                value = lut[value];
            if (needsBrightness)
                value = (byte)(value * bright + 0.5f);
            dst[i] = value;
        }
    }

    private static byte[] BuildGammaLut(float gamma)
    {
        var lut = new byte[256];
        for (var i = 0; i < 256; i++)
            lut[i] = (byte)Math.Clamp(MathF.Round(MathF.Pow(i / 255f, gamma) * 255f), 0, 255);
        return lut;
    }

    private void ThrowIfDisposed()
    {
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(disposed, this);
#else
        if (disposed)
            throw new ObjectDisposedException(nameof(Pi5Matrix));
#endif
    }

    private static void ValidateGeometryOptions(Pi5MatrixGeometryOptions geometry)
    {
        if (geometry.LaneCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(geometry),
                "LaneCount must be positive.");
        }

        var hasPixelMap = geometry.PixelMap is { Length: > 0 };
        if (!hasPixelMap)
        {
            if (geometry.LaneCount != 2)
            {
                throw new ArgumentException(
                    "LaneCount values other than 2 require a custom PixelMap. Use Pi5MatrixPixelMappers.SimpleMultilane(...) for Adafruit's documented multi-lane layout.",
                    nameof(geometry));
            }

            return;
        }

        if (geometry.Orientation != Pi5MatrixOrientation.Normal)
        {
            throw new ArgumentException(
                "Custom PixelMap configurations currently require Orientation.Normal. Encode rotation directly in the pixel map.",
                nameof(geometry));
        }

        var totalPixels = checked(geometry.Width * geometry.Height);
        var pixelMap = geometry.PixelMap!;
        if (pixelMap.Length != totalPixels)
        {
            throw new ArgumentException(
                "Custom PixelMap configurations must contain one entry per framebuffer pixel.",
                nameof(geometry));
        }

        var nLines = checked(geometry.LaneCount << geometry.AddressLineCount);
        if (totalPixels % nLines != 0)
        {
            throw new ArgumentException(
                "Width * Height must be divisible by LaneCount << AddressLineCount for custom pixel maps.",
                nameof(geometry));
        }

        foreach (var pixelIndex in pixelMap)
        {
            if ((uint)pixelIndex >= (uint)totalPixels)
            {
                throw new ArgumentException(
                    "Custom PixelMap entries must fall within the framebuffer bounds.",
                    nameof(geometry));
            }
        }
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (handle != IntPtr.Zero)
        {
            Pi5Bindings.pi5_matrix_delete(handle);
            handle = IntPtr.Zero;
        }

        if (pinnedFrontBuffer.IsAllocated)
            pinnedFrontBuffer.Free();

        disposed = true;
    }

    ~Pi5Matrix()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
