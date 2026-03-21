using System;
using System.Reflection;
using Xunit;

namespace Pi5MatrixSharp.Tests;

public class GammaLutTests
{
    private static byte[] BuildGammaLut(float gamma)
    {
        var method = typeof(Pi5Matrix).GetMethod("BuildGammaLut", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return (byte[])method!.Invoke(null, [gamma])!;
    }

    [Fact]
    public void GammaLut_HasExactly256Entries()
    {
        var lut = BuildGammaLut(2.5f);
        Assert.Equal(256, lut.Length);
    }

    [Fact]
    public void GammaLut_ZeroMapsToZero()
    {
        var lut = BuildGammaLut(2.5f);
        Assert.Equal(0, lut[0]);
    }

    [Fact]
    public void GammaLut_255MapsTo255()
    {
        var lut = BuildGammaLut(2.5f);
        Assert.Equal(255, lut[255]);
    }

    [Fact]
    public void GammaLut_IsMonotonicallyNonDecreasing()
    {
        var lut = BuildGammaLut(2.5f);
        for (var i = 1; i < 256; i++)
            Assert.True(lut[i] >= lut[i - 1], $"LUT[{i}]={lut[i]} < LUT[{i - 1}]={lut[i - 1]}");
    }

    [Fact]
    public void GammaLut_MidpointIsDimmedForGammaAboveOne()
    {
        var lut = BuildGammaLut(2.5f);
        // With gamma 2.5, input 128 should map well below 128
        Assert.True(lut[128] < 100, $"Expected gamma-corrected midpoint < 100, got {lut[128]}");
    }

    [Fact]
    public void GammaLut_GammaOneIsIdentity()
    {
        var lut = BuildGammaLut(1.0f);
        for (var i = 0; i < 256; i++)
            Assert.Equal(i, lut[i]);
    }

    [Theory]
    [InlineData(1.8f)]
    [InlineData(2.2f)]
    [InlineData(2.5f)]
    [InlineData(2.8f)]
    public void GammaLut_CommonExponentsProduceValidRange(float gamma)
    {
        var lut = BuildGammaLut(gamma);
        foreach (var value in lut)
        {
            Assert.InRange(value, (byte)0, (byte)255);
        }
    }
}
