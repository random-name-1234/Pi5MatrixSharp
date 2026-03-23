namespace Pi5MatrixSharp;

/// <summary>
/// Panel geometry and timing-related options for the Pi 5 backend.
/// </summary>
public struct Pi5MatrixGeometryOptions
{
    /// <summary>
    /// Total framebuffer width in pixels.
    /// </summary>
    public int Width = 64;

    /// <summary>
    /// Total framebuffer height in pixels.
    /// </summary>
    public int Height = 32;

    /// <summary>
    /// Number of HUB75 address lines connected to the panel chain.
    /// </summary>
    public int AddressLineCount = 4;

    /// <summary>
    /// Controls the default simple mapper used for the stable 2-lane path.
    /// Ignored when <see cref="PixelMap"/> is provided.
    /// </summary>
    public bool Serpentine = true;

    /// <summary>
    /// Controls the default simple mapper orientation for the stable 2-lane path.
    /// Custom pixel maps currently require <see cref="Pi5MatrixOrientation.Normal"/>.
    /// </summary>
    public Pi5MatrixOrientation Orientation = Pi5MatrixOrientation.Normal;

    /// <summary>
    /// Number of PWM bit planes sent to the panel.
    /// </summary>
    public int PlaneCount = 10;

    /// <summary>
    /// Number of temporal dither planes sent less frequently.
    /// </summary>
    public int TemporalPlaneCount = 2;

    /// <summary>
    /// Number of color lanes exposed by the active pinout.
    /// The stable single-connector path uses 2 lanes.
    /// Values above 2 are experimental and currently require <see cref="PixelMap"/>.
    /// </summary>
    public int LaneCount = 2;

    /// <summary>
    /// Optional custom matrix pixel map.
    /// When provided, the native backend uses Piomatter's custom-map constructor.
    /// This enables experimental multi-lane setups such as Active3.
    /// </summary>
    public int[]? PixelMap;

    public Pi5MatrixGeometryOptions()
    {
    }

    /// <summary>
    /// Creates geometry configured for Adafruit's documented simple multi-lane layout.
    /// This path is experimental in Pi5MatrixSharp until it has been validated on real
    /// multi-output hardware.
    /// </summary>
    public static Pi5MatrixGeometryOptions CreateSimpleMultilane(
        int width,
        int height,
        int addressLineCount,
        int laneCount,
        int planeCount = 10,
        int temporalPlaneCount = 2) =>
        new()
        {
            Width = width,
            Height = height,
            AddressLineCount = addressLineCount,
            Serpentine = false,
            Orientation = Pi5MatrixOrientation.Normal,
            PlaneCount = planeCount,
            TemporalPlaneCount = temporalPlaneCount,
            LaneCount = laneCount,
            PixelMap = Pi5MatrixPixelMappers.SimpleMultilane(
                width,
                height,
                addressLineCount,
                laneCount)
        };
}
