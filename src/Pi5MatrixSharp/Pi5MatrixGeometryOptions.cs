namespace Pi5MatrixSharp;

/// <summary>
/// Panel geometry and timing-related options for the Pi 5 backend.
/// </summary>
public struct Pi5MatrixGeometryOptions
{
    public int Width = 64;
    public int Height = 32;
    public int AddressLineCount = 4;
    public bool Serpentine = true;
    public Pi5MatrixOrientation Orientation = Pi5MatrixOrientation.Normal;
    public int PlaneCount = 10;
    public int TemporalPlaneCount = 2;

    public Pi5MatrixGeometryOptions()
    {
    }
}
