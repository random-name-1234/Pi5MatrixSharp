using System;

namespace Pi5MatrixSharp;

/// <summary>
/// Helper pixel-mapping functions for advanced geometry scenarios.
/// </summary>
public static class Pi5MatrixPixelMappers
{
    /// <summary>
    /// Builds Adafruit's documented simple multi-lane mapper for 4+ lane setups.
    /// This is suitable for experimental Active3/triple-output configurations.
    /// </summary>
    public static int[] SimpleMultilane(
        int width,
        int height,
        int addressLineCount,
        int laneCount)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        if (addressLineCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(addressLineCount), "Address line count must be positive.");
        if (laneCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(laneCount), "Lane count must be positive.");

        var expectedHeight = checked(laneCount << addressLineCount);
        if (height != expectedHeight)
        {
            throw new ArgumentOutOfRangeException(
                nameof(height),
                $"Height {height} does not match the simple multi-lane layout height {expectedHeight} for {laneCount} lanes and {addressLineCount} address lines.");
        }

        var nAddr = 1 << addressLineCount;
        var map = new int[checked(width * height)];
        var index = 0;

        for (var addr = 0; addr < nAddr; addr++)
        for (var x = 0; x < width; x++)
        for (var lane = 0; lane < laneCount; lane++)
        {
            var y = addr + lane * nAddr;
            map[index++] = checked(x + width * y);
        }

        return map;
    }
}
