using Pi5MatrixSharp;

var useExperimentalCustomMap = args.Contains("--experimental-custom-map", StringComparer.OrdinalIgnoreCase);
var frameLimit = args
    .Select(static arg => arg.Split('=', 2))
    .Where(static parts => parts.Length == 2 && parts[0].Equals("--frames", StringComparison.OrdinalIgnoreCase))
    .Select(static parts => int.TryParse(parts[1], out var value) ? value : (int?)null)
    .FirstOrDefault(static value => value.HasValue);

var options = new Pi5MatrixOptions
{
    Pinout = Pi5MatrixPinout.AdafruitMatrixBonnet,
    Geometry = useExperimentalCustomMap
        ? Pi5MatrixGeometryOptions.CreateSimpleMultilane(
            width: 64,
            height: 32,
            addressLineCount: 4,
            laneCount: 2,
            planeCount: 10,
            temporalPlaneCount: 2)
        : new Pi5MatrixGeometryOptions
        {
            Width = 64,
            Height = 32,
            AddressLineCount = 4,
            Serpentine = true,
            Orientation = Pi5MatrixOrientation.Normal,
            PlaneCount = 10,
            TemporalPlaneCount = 2
        }
};

using var matrix = new Pi5Matrix(options);
Console.WriteLine(useExperimentalCustomMap
    ? "Running sample with experimental custom-map geometry."
    : "Running sample with stable simple geometry.");

matrix.GammaCorrection = true;
matrix.Brightness = 0.5f;
Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    Environment.Exit(0);
};

Console.WriteLine(matrix);
for (var frame = 0; !frameLimit.HasValue || frame < frameLimit.Value; frame++)
{
    for (var y = 0; y < matrix.Height; y++)
    for (var x = 0; x < matrix.Width; x++)
    {
        var r = (byte)((x * 255) / Math.Max(1, matrix.Width - 1));
        var g = (byte)((y * 255) / Math.Max(1, matrix.Height - 1));
        var b = (byte)((frame * 4) % 256);
        matrix.SetPixel(x, y, r, g, b);
    }

    matrix.Show();

    // Print diagnostics every 100 frames
    if (frame % 100 == 0 && frame > 0)
    {
        var diag = matrix.GetDiagnostics();
        Console.WriteLine($"Frame {frame}: {diag.HardwareFps:F0}fps, " +
                          $"show={diag.LastShowDurationMs:F2}ms, " +
                          $"brightness={diag.Brightness:P0}");
    }

    Thread.Sleep(33);
}
