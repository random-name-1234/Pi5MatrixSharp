using Pi5MatrixSharp;

const int width = 64;
const int laneCount = 6;
const int addressLineCount = 5;
var height = laneCount << addressLineCount;

var options = new Pi5MatrixOptions
{
    Pinout = Pi5MatrixPinout.Active3,
    Geometry = Pi5MatrixGeometryOptions.CreateSimpleMultilane(
        width: width,
        height: height,
        addressLineCount: addressLineCount,
        laneCount: laneCount,
        planeCount: 10,
        temporalPlaneCount: 4)
};

using var matrix = new Pi5Matrix(options);
Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    Environment.Exit(0);
};

for (var frame = 0; ; frame++)
{
    matrix.Clear();

    DrawRectangle(8, 8, 48, 48, 0, 136, 0);
    DrawCircle(32, 96, 22, 136, 0, 0);
    DrawTriangle(32, 136, 54, 180, 10, 180, 0, 0, 136);

    matrix.Show();
    Thread.Sleep(33);
}

void DrawRectangle(int x0, int y0, int x1, int y1, byte r, byte g, byte b)
{
    for (var y = y0; y <= y1; y++)
    for (var x = x0; x <= x1; x++)
        matrix.SetPixel(x, y, r, g, b);
}

void DrawCircle(int centerX, int centerY, int radius, byte r, byte g, byte b)
{
    for (var y = centerY - radius; y <= centerY + radius; y++)
    for (var x = centerX - radius; x <= centerX + radius; x++)
    {
        var dx = x - centerX;
        var dy = y - centerY;
        if (dx * dx + dy * dy <= radius * radius)
            matrix.SetPixel(x, y, r, g, b);
    }
}

void DrawTriangle(
    int x0,
    int y0,
    int x1,
    int y1,
    int x2,
    int y2,
    byte r,
    byte g,
    byte b)
{
    var minX = Math.Min(x0, Math.Min(x1, x2));
    var maxX = Math.Max(x0, Math.Max(x1, x2));
    var minY = Math.Min(y0, Math.Min(y1, y2));
    var maxY = Math.Max(y0, Math.Max(y1, y2));

    for (var y = minY; y <= maxY; y++)
    for (var x = minX; x <= maxX; x++)
    {
        if (IsInsideTriangle(x, y, x0, y0, x1, y1, x2, y2))
            matrix.SetPixel(x, y, r, g, b);
    }
}

static bool IsInsideTriangle(
    int x,
    int y,
    int x0,
    int y0,
    int x1,
    int y1,
    int x2,
    int y2)
{
    var d1 = Sign(x, y, x0, y0, x1, y1);
    var d2 = Sign(x, y, x1, y1, x2, y2);
    var d3 = Sign(x, y, x2, y2, x0, y0);
    var hasNegative = d1 < 0 || d2 < 0 || d3 < 0;
    var hasPositive = d1 > 0 || d2 > 0 || d3 > 0;
    return !(hasNegative && hasPositive);
}

static int Sign(int px, int py, int ax, int ay, int bx, int by) =>
    (px - bx) * (ay - by) - (ax - bx) * (py - by);
