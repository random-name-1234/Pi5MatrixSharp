using System.Runtime.InteropServices;

namespace Pi5MatrixSharp;

[StructLayout(LayoutKind.Sequential)]
internal struct InternalPi5MatrixGeometryOptions
{
    public int width;
    public int height;
    public int n_addr_lines;
    public byte serpentine;
    private readonly byte reserved0;
    private readonly byte reserved1;
    private readonly byte reserved2;
    public int rotation;
    public int n_planes;
    public int n_temporal_planes;

    public InternalPi5MatrixGeometryOptions(Pi5MatrixGeometryOptions options)
    {
        width = options.Width;
        height = options.Height;
        n_addr_lines = options.AddressLineCount;
        serpentine = (byte)(options.Serpentine ? 1 : 0);
        reserved0 = 0;
        reserved1 = 0;
        reserved2 = 0;
        rotation = (int)options.Orientation;
        n_planes = options.PlaneCount;
        n_temporal_planes = options.TemporalPlaneCount;
    }
}
