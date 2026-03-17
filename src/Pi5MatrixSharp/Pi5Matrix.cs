using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Pi5MatrixSharp;

/// <summary>
/// Managed wrapper over <c>libpi5matrix.so</c> for Raspberry Pi 5 HUB75 panels.
/// </summary>
public sealed class Pi5Matrix : IDisposable
{
    private readonly GCHandle pinnedFramebuffer;
    private IntPtr handle;
    private bool disposed;

    /// <summary>
    /// Creates a new matrix instance and allocates a pinned framebuffer for it.
    /// </summary>
    public Pi5Matrix(Pi5MatrixOptions options)
    {
        Options = options;

        var geometry = new InternalPi5MatrixGeometryOptions(options.Geometry);
        var expectedSize = checked((int)Pi5Bindings.pi5_matrix_expected_framebuffer_size(
            (int)options.Colorspace,
            options.Geometry.Width,
            options.Geometry.Height));
        if (expectedSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "The selected colorspace/geometry produced an invalid framebuffer size.");

        FrameBuffer = new byte[expectedSize];
        pinnedFramebuffer = GCHandle.Alloc(FrameBuffer, GCHandleType.Pinned);

        handle = Pi5Bindings.pi5_matrix_create_from_buffer(
            (int)options.Colorspace,
            (int)options.Pinout,
            pinnedFramebuffer.AddrOfPinnedObject(),
            (nuint)FrameBuffer.Length,
            ref geometry);

        if (handle == IntPtr.Zero)
        {
            pinnedFramebuffer.Free();
            throw new InvalidOperationException(
                $"Failed to create Pi 5 matrix: {Pi5Bindings.GetLastErrorMessage()}");
        }
    }

    /// <summary>
    /// Options used to create this matrix instance.
    /// </summary>
    public Pi5MatrixOptions Options { get; }

    /// <summary>
    /// Backing framebuffer in the configured colorspace.
    /// </summary>
    public byte[] FrameBuffer { get; }

    public int Width => Options.Geometry.Width;

    public int Height => Options.Geometry.Height;

    public double FramesPerSecond
    {
        get
        {
            ThrowIfDisposed();
            return Pi5Bindings.pi5_matrix_get_fps(handle);
        }
    }

    /// <summary>
    /// Returns the underlying framebuffer as a mutable span.
    /// </summary>
    public Span<byte> GetFrameBuffer() => FrameBuffer;

    /// <summary>
    /// Clears the current framebuffer contents to black.
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();
        Array.Clear(FrameBuffer);
    }

    /// <summary>
    /// Copies a packed RGB888 frame into the internal framebuffer.
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
    /// Sets one pixel in the packed RGB888 framebuffer.
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
    /// Pushes the current framebuffer to the matrix hardware.
    /// </summary>
    public void Show()
    {
        ThrowIfDisposed();

        var err = Pi5Bindings.pi5_matrix_show(handle);
        if (err != 0)
            throw new IOException($"Pi 5 matrix show() failed with errno {err}: {Pi5Bindings.GetLastErrorMessage()}");
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(Pi5Matrix));
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

        if (pinnedFramebuffer.IsAllocated)
            pinnedFramebuffer.Free();

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
