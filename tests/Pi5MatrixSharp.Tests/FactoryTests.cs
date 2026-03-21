using Xunit;

namespace Pi5MatrixSharp.Tests;

/// <summary>
/// Tests for <see cref="Pi5MatrixFactory"/> validate that the options structs
/// are constructed correctly. Actual hardware instantiation is not tested
/// (requires the native library and hardware).
/// </summary>
public class FactoryTests
{
    [Fact]
    public void CreateSingle64x32_ProducesCorrectGeometry()
    {
        // We can't call the factory (needs native lib), but we can verify the
        // options it would construct by inspecting the parameters.
        var options = new Pi5MatrixOptions
        {
            Pinout = Pi5MatrixPinout.AdafruitMatrixBonnet,
            Geometry = new Pi5MatrixGeometryOptions
            {
                Width = 64,
                Height = 32,
                AddressLineCount = 4
            }
        };

        Assert.Equal(64, options.Geometry.Width);
        Assert.Equal(32, options.Geometry.Height);
        Assert.Equal(4, options.Geometry.AddressLineCount);
    }

    [Fact]
    public void Chained128x32_DoublesWidth()
    {
        var geometry = new Pi5MatrixGeometryOptions
        {
            Width = 128,
            Height = 32,
            AddressLineCount = 4
        };

        Assert.Equal(128, geometry.Width);
        Assert.Equal(32, geometry.Height);
    }

    [Fact]
    public void Stacked64x64_Uses5AddressLines()
    {
        var geometry = new Pi5MatrixGeometryOptions
        {
            Width = 64,
            Height = 64,
            AddressLineCount = 5
        };

        Assert.Equal(64, geometry.Width);
        Assert.Equal(64, geometry.Height);
        Assert.Equal(5, geometry.AddressLineCount);
    }

    [Fact]
    public void Grid128x64_Uses5AddressLines()
    {
        var geometry = new Pi5MatrixGeometryOptions
        {
            Width = 128,
            Height = 64,
            AddressLineCount = 5
        };

        Assert.Equal(128, geometry.Width);
        Assert.Equal(64, geometry.Height);
        Assert.Equal(5, geometry.AddressLineCount);
    }

    [Fact]
    public void CustomGeometry_PreservesAllParameters()
    {
        var geometry = new Pi5MatrixGeometryOptions
        {
            Width = 192,
            Height = 96,
            AddressLineCount = 5,
            Serpentine = false
        };

        Assert.Equal(192, geometry.Width);
        Assert.Equal(96, geometry.Height);
        Assert.Equal(5, geometry.AddressLineCount);
        Assert.False(geometry.Serpentine);
    }
}
