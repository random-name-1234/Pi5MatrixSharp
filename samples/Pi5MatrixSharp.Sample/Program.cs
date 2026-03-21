using Pi5MatrixSharp;

// Use the factory for a common single-panel setup
using var matrix = Pi5MatrixFactory.CreateSingle64x32();

// Enable gamma correction for accurate colours on LED panels
matrix.GammaCorrection = true;

// Start at half brightness
matrix.Brightness = 0.5f;

Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    Environment.Exit(0);
};

Console.WriteLine(matrix);

for (var frame = 0; ; frame++)
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
