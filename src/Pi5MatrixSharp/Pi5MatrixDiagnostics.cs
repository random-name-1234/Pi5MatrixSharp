namespace Pi5MatrixSharp;

/// <summary>
/// Diagnostic snapshot of a <see cref="Pi5Matrix"/> instance.
/// </summary>
public readonly record struct Pi5MatrixDiagnostics(
    int Width,
    int Height,
    Pi5Colorspace Colorspace,
    Pi5MatrixPinout Pinout,
    double HardwareFps,
    long ShowCount,
    double LastShowDurationMs,
    int FrameBufferSize,
    float Brightness,
    bool GammaCorrection);
