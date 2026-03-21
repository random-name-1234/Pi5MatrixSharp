namespace Pi5MatrixSharp;

/// <summary>
/// Factory methods for common multi-panel configurations.
/// </summary>
public static class Pi5MatrixFactory
{
    /// <summary>
    /// Creates a matrix for a single 64x32 panel using the Adafruit Matrix Bonnet.
    /// </summary>
    public static Pi5Matrix CreateSingle64x32(
        Pi5MatrixPinout pinout = Pi5MatrixPinout.AdafruitMatrixBonnet)
    {
        return new Pi5Matrix(new Pi5MatrixOptions
        {
            Pinout = pinout,
            Geometry = new Pi5MatrixGeometryOptions
            {
                Width = 64,
                Height = 32,
                AddressLineCount = 4
            }
        });
    }

    /// <summary>
    /// Creates a matrix for two 64x32 panels chained side-by-side (128x32).
    /// </summary>
    public static Pi5Matrix CreateChained128x32(
        Pi5MatrixPinout pinout = Pi5MatrixPinout.AdafruitMatrixBonnet)
    {
        return new Pi5Matrix(new Pi5MatrixOptions
        {
            Pinout = pinout,
            Geometry = new Pi5MatrixGeometryOptions
            {
                Width = 128,
                Height = 32,
                AddressLineCount = 4
            }
        });
    }

    /// <summary>
    /// Creates a matrix for two 64x32 panels stacked vertically (64x64).
    /// Requires 5 address lines for the taller addressable range.
    /// </summary>
    public static Pi5Matrix CreateStacked64x64(
        Pi5MatrixPinout pinout = Pi5MatrixPinout.AdafruitMatrixBonnet)
    {
        return new Pi5Matrix(new Pi5MatrixOptions
        {
            Pinout = pinout,
            Geometry = new Pi5MatrixGeometryOptions
            {
                Width = 64,
                Height = 64,
                AddressLineCount = 5
            }
        });
    }

    /// <summary>
    /// Creates a matrix for four 64x32 panels in a 2x2 grid (128x64).
    /// </summary>
    public static Pi5Matrix CreateGrid128x64(
        Pi5MatrixPinout pinout = Pi5MatrixPinout.AdafruitMatrixBonnet)
    {
        return new Pi5Matrix(new Pi5MatrixOptions
        {
            Pinout = pinout,
            Geometry = new Pi5MatrixGeometryOptions
            {
                Width = 128,
                Height = 64,
                AddressLineCount = 5
            }
        });
    }

    /// <summary>
    /// Creates a matrix with a custom panel arrangement.
    /// </summary>
    /// <param name="width">Total width in pixels.</param>
    /// <param name="height">Total height in pixels.</param>
    /// <param name="addressLineCount">Number of address lines (4 for 32-row panels, 5 for 64-row).</param>
    /// <param name="pinout">Hardware pinout.</param>
    /// <param name="serpentine">Whether panels are wired in serpentine order.</param>
    public static Pi5Matrix CreateCustom(
        int width,
        int height,
        int addressLineCount = 4,
        Pi5MatrixPinout pinout = Pi5MatrixPinout.AdafruitMatrixBonnet,
        bool serpentine = true)
    {
        return new Pi5Matrix(new Pi5MatrixOptions
        {
            Pinout = pinout,
            Geometry = new Pi5MatrixGeometryOptions
            {
                Width = width,
                Height = height,
                AddressLineCount = addressLineCount,
                Serpentine = serpentine
            }
        });
    }
}
