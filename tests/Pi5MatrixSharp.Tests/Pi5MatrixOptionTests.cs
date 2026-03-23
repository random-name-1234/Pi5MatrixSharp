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
        Assert.Equal(2, geometry.LaneCount);
        Assert.Null(geometry.PixelMap);
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
            TemporalPlaneCount = 3,
            LaneCount = 6,
            PixelMap = [0, 1, 2]
        };

        var native = new InternalPi5MatrixGeometryOptions(managed, (IntPtr)1234, 3);

        Assert.Equal(128, native.width);
        Assert.Equal(64, native.height);
        Assert.Equal(5, native.n_addr_lines);
        Assert.Equal((byte)0, native.serpentine);
        Assert.Equal((int)Pi5MatrixOrientation.Ccw, native.rotation);
        Assert.Equal(11, native.n_planes);
        Assert.Equal(3, native.n_temporal_planes);
        Assert.Equal((nuint)6, native.n_lanes);
        Assert.Equal((nuint)3, native.pixel_map_length);
        Assert.Equal((IntPtr)1234, native.pixel_map);
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

    [Fact]
    public void SimpleMultilaneMapperMatchesAdafruitLayout()
    {
        var map = Pi5MatrixPixelMappers.SimpleMultilane(
            width: 2,
            height: 4,
            addressLineCount: 1,
            laneCount: 2);

        Assert.Equal([0, 4, 1, 5, 2, 6, 3, 7], map);
    }

    [Fact]
    public void CreateSimpleMultilaneBuildsExperimentalGeometry()
    {
        var geometry = Pi5MatrixGeometryOptions.CreateSimpleMultilane(
            width: 64,
            height: 192,
            addressLineCount: 5,
            laneCount: 6,
            temporalPlaneCount: 4);

        Assert.Equal(64, geometry.Width);
        Assert.Equal(192, geometry.Height);
        Assert.Equal(6, geometry.LaneCount);
        Assert.False(geometry.Serpentine);
        Assert.Equal(Pi5MatrixOrientation.Normal, geometry.Orientation);
        Assert.Equal(4, geometry.TemporalPlaneCount);
        Assert.NotNull(geometry.PixelMap);
        Assert.Equal(64 * 192, geometry.PixelMap!.Length);
    }

    [Fact]
    public void SimpleMultilaneRejectsMismatchedHeight()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Pi5MatrixPixelMappers.SimpleMultilane(
                width: 64,
                height: 32,
                addressLineCount: 5,
                laneCount: 6));

        Assert.Contains("does not match", ex.Message);
    }
}
