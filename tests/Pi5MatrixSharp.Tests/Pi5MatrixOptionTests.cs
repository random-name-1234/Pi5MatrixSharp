using Xunit;

namespace Pi5MatrixSharp.Tests;

public class Pi5MatrixOptionTests
{
    [Fact]
    public void GeometryDefaultsMatchCommon64x32Panel()
    {
        var geometry = new Pi5MatrixGeometryOptions();

        Assert.Equal(64, geometry.Width);
        Assert.Equal(32, geometry.Height);
        Assert.Equal(4, geometry.AddressLineCount);
        Assert.True(geometry.Serpentine);
        Assert.Equal(Pi5MatrixOrientation.Normal, geometry.Orientation);
        Assert.Equal(10, geometry.PlaneCount);
        Assert.Equal(2, geometry.TemporalPlaneCount);
    }

    [Fact]
    public void NativeGeometryMappingPreservesManagedValues()
    {
        var managed = new Pi5MatrixGeometryOptions
        {
            Width = 128,
            Height = 64,
            AddressLineCount = 5,
            Serpentine = false,
            Orientation = Pi5MatrixOrientation.Ccw,
            PlaneCount = 11,
            TemporalPlaneCount = 3
        };

        var native = new InternalPi5MatrixGeometryOptions(managed);

        Assert.Equal(128, native.width);
        Assert.Equal(64, native.height);
        Assert.Equal(5, native.n_addr_lines);
        Assert.Equal((byte)0, native.serpentine);
        Assert.Equal((int)Pi5MatrixOrientation.Ccw, native.rotation);
        Assert.Equal(11, native.n_planes);
        Assert.Equal(3, native.n_temporal_planes);
    }

    [Fact]
    public void NativeGeometryMappingSerpentineTrueIsOne()
    {
        var managed = new Pi5MatrixGeometryOptions { Serpentine = true };
        var native = new InternalPi5MatrixGeometryOptions(managed);

        Assert.Equal((byte)1, native.serpentine);
    }

    [Fact]
    public void MatrixOptionsDefaultToPackedRgbOnAdafruitBonnet()
    {
        var options = new Pi5MatrixOptions();

        Assert.Equal(Pi5Colorspace.Rgb888Packed, options.Colorspace);
        Assert.Equal(Pi5MatrixPinout.AdafruitMatrixBonnet, options.Pinout);
        Assert.Equal(64, options.Geometry.Width);
        Assert.Equal(32, options.Geometry.Height);
    }

    [Theory]
    [InlineData(Pi5MatrixOrientation.Normal, 0)]
    [InlineData(Pi5MatrixOrientation.R180, 1)]
    [InlineData(Pi5MatrixOrientation.Ccw, 2)]
    [InlineData(Pi5MatrixOrientation.Cw, 3)]
    public void OrientationEnumValuesMatchNativeConstants(Pi5MatrixOrientation orientation, int expected)
    {
        Assert.Equal(expected, (int)orientation);
    }

    [Theory]
    [InlineData(Pi5Colorspace.Rgb565, 0)]
    [InlineData(Pi5Colorspace.Rgb888, 1)]
    [InlineData(Pi5Colorspace.Rgb888Packed, 2)]
    public void ColorspaceEnumValuesMatchNativeConstants(Pi5Colorspace colorspace, int expected)
    {
        Assert.Equal(expected, (int)colorspace);
    }

    [Theory]
    [InlineData(Pi5MatrixPinout.AdafruitMatrixBonnet, 0)]
    [InlineData(Pi5MatrixPinout.AdafruitMatrixBonnetBgr, 1)]
    [InlineData(Pi5MatrixPinout.Active3, 2)]
    [InlineData(Pi5MatrixPinout.Active3Bgr, 3)]
    public void PinoutEnumValuesMatchNativeConstants(Pi5MatrixPinout pinout, int expected)
    {
        Assert.Equal(expected, (int)pinout);
    }
}
