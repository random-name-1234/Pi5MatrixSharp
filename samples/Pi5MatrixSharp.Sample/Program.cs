using Pi5MatrixSharp;

var options = new Pi5MatrixOptions
{
    Pinout = Pi5MatrixPinout.AdafruitMatrixBonnet,
    Geometry = new Pi5MatrixGeometryOptions
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
Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    Environment.Exit(0);
};

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
    Thread.Sleep(33);
}
