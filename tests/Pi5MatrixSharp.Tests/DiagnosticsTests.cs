using Xunit;

namespace Pi5MatrixSharp.Tests;

public class DiagnosticsTests
{
    [Fact]
    public void DiagnosticsRecord_PreservesAllValues()
    {
        var diag = new Pi5MatrixDiagnostics(
            Width: 64,
            Height: 32,
            Colorspace: Pi5Colorspace.Rgb888Packed,
            Pinout: Pi5MatrixPinout.AdafruitMatrixBonnet,
            HardwareFps: 120.5,
            ShowCount: 42,
            LastShowDurationMs: 1.23,
            FrameBufferSize: 6144,
            Brightness: 0.75f,
            GammaCorrection: true);

        Assert.Equal(64, diag.Width);
        Assert.Equal(32, diag.Height);
        Assert.Equal(Pi5Colorspace.Rgb888Packed, diag.Colorspace);
        Assert.Equal(Pi5MatrixPinout.AdafruitMatrixBonnet, diag.Pinout);
        Assert.Equal(120.5, diag.HardwareFps);
        Assert.Equal(42, diag.ShowCount);
        Assert.Equal(1.23, diag.LastShowDurationMs);
        Assert.Equal(6144, diag.FrameBufferSize);
        Assert.Equal(0.75f, diag.Brightness);
        Assert.True(diag.GammaCorrection);
    }

    [Fact]
    public void DiagnosticsRecord_SupportsEquality()
    {
        var a = new Pi5MatrixDiagnostics(64, 32, Pi5Colorspace.Rgb888Packed,
            Pi5MatrixPinout.AdafruitMatrixBonnet, 120, 0, 0, 6144, 1f, false);
        var b = new Pi5MatrixDiagnostics(64, 32, Pi5Colorspace.Rgb888Packed,
            Pi5MatrixPinout.AdafruitMatrixBonnet, 120, 0, 0, 6144, 1f, false);

        Assert.Equal(a, b);
    }
}
