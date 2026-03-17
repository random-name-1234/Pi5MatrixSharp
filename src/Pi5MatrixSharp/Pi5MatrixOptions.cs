namespace Pi5MatrixSharp;

/// <summary>
/// Top-level options used when creating a Pi 5 matrix instance.
/// </summary>
public struct Pi5MatrixOptions
{
    public Pi5Colorspace Colorspace = Pi5Colorspace.Rgb888Packed;
    public Pi5MatrixPinout Pinout = Pi5MatrixPinout.AdafruitMatrixBonnet;
    public Pi5MatrixGeometryOptions Geometry = new();

    public Pi5MatrixOptions()
    {
    }
}
