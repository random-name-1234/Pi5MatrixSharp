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
    public nuint n_lanes;
    public nuint pixel_map_length;
    public IntPtr pixel_map;

    public InternalPi5MatrixGeometryOptions(Pi5MatrixGeometryOptions options)
        : this(options, IntPtr.Zero, 0)
    {
    }

    public InternalPi5MatrixGeometryOptions(
        Pi5MatrixGeometryOptions options,
        IntPtr pixelMap,
        nuint pixelMapLength)
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
        n_lanes = checked((nuint)options.LaneCount);
        pixel_map_length = pixelMapLength;
        pixel_map = pixelMap;
    }
}
