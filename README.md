![Pi5MatrixSharp logo](https://raw.githubusercontent.com/random-name-1234/Pi5MatrixSharp/main/assets/pi5matrixsharp-logo.svg)

# Pi5MatrixSharp

[![CI](https://github.com/random-name-1234/Pi5MatrixSharp/actions/workflows/ci.yml/badge.svg)](https://github.com/random-name-1234/Pi5MatrixSharp/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Pi5MatrixSharp)](https://www.nuget.org/packages/Pi5MatrixSharp/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Pi5MatrixSharp)](https://www.nuget.org/packages/Pi5MatrixSharp/)
[![License](https://img.shields.io/github/license/random-name-1234/Pi5MatrixSharp)](https://github.com/random-name-1234/Pi5MatrixSharp/blob/main/LICENSE)

`Pi5MatrixSharp` is a C# wrapper for driving HUB75 RGB LED matrix panels on the Raspberry Pi 5 using Adafruit's Piomatter backend.

It is aimed at the "keep my app in C#" case: render however you like in managed code, then push the final frame to the panel through a small native shim.

## Status

- Raspberry Pi 5 only
- Linux ARM64 only
- Native backend bundled as `libpi5matrix.so`
- Tested against Adafruit's `Adafruit_Blinka_Raspberry_Pi5_Piomatter` at commit `9ce4965a3fddf5b44c9da6c8dc3738cfe0403028`

## Install

```bash
dotnet add package Pi5MatrixSharp
```

Package page: <https://www.nuget.org/packages/Pi5MatrixSharp/>

## Quick Start

```csharp
using Pi5MatrixSharp;

// Factory method for the most common single-panel setup
using var matrix = Pi5MatrixFactory.CreateSingle64x32();

matrix.SetPixel(0, 0, 255, 0, 0);
matrix.SetPixel(1, 0, 0, 255, 0);
matrix.SetPixel(2, 0, 0, 0, 255);
matrix.Show();
```

Or configure everything manually:

```csharp
using var matrix = new Pi5Matrix(new Pi5MatrixOptions
{
    Pinout = Pi5MatrixPinout.AdafruitMatrixBonnet,
    Geometry = new Pi5MatrixGeometryOptions
    {
        Width = 64,
        Height = 32,
        AddressLineCount = 4
    }
});
```

For a runnable example, see [samples/Pi5MatrixSharp.Sample](https://github.com/random-name-1234/Pi5MatrixSharp/tree/main/samples/Pi5MatrixSharp.Sample).

## Features

### Brightness Control

Adjust the master brightness at runtime. Applied during `Show()` with no native overhead.

```csharp
matrix.Brightness = 0.5f;  // 50% brightness
```

### Gamma Correction

LED panels have non-linear brightness response. Enable gamma correction for accurate colours:

```csharp
matrix.GammaCorrection = true;     // default gamma 2.5
matrix.GammaExponent = 2.2f;       // or choose your own
```

### Double Buffering

Write pixels to the back buffer, then call `Show()` to push them to hardware. The front buffer (pinned in memory for native access) is updated atomically with brightness and gamma applied. No tearing.

```csharp
matrix.SetPixel(10, 5, 255, 128, 0);
matrix.Show();  // back buffer -> front buffer -> hardware
```

### CopyRgba32

Drop-in for SixLabors.ImageSharp `Rgba32` pixel data. Alpha is discarded automatically.

```csharp
var rgba = new byte[64 * 32 * 4];
image.CopyPixelDataTo(rgba);
matrix.CopyRgba32(rgba);
matrix.Show();
```

### Thread Safety

`Show()` is internally synchronised. Safe to call from a render thread while another thread modifies the back buffer.

### Chained Panels

Factory methods for common multi-panel configurations:

```csharp
// Two panels side-by-side (128x32)
using var wide = Pi5MatrixFactory.CreateChained128x32();

// Two panels stacked (64x64)
using var tall = Pi5MatrixFactory.CreateStacked64x64();

// Four panels in a 2x2 grid (128x64)
using var grid = Pi5MatrixFactory.CreateGrid128x64();

// Arbitrary arrangement
using var custom = Pi5MatrixFactory.CreateCustom(192, 96, addressLineCount: 5);
```

### Diagnostics

```csharp
Console.WriteLine(matrix);
// Pi5Matrix 64x32 Rgb888Packed @ 120fps

var diag = matrix.GetDiagnostics();
Console.WriteLine($"Show count: {diag.ShowCount}, last show: {diag.LastShowDurationMs:F2}ms");
```

## Requirements

- Raspberry Pi 5
- 64-bit Raspberry Pi OS with `/dev/pio0`
- User in the `gpio` group
- A supported pinout:
  - `AdafruitMatrixBonnet`
  - `AdafruitMatrixBonnetBgr`
  - `Active3`
  - `Active3Bgr`

## Building The Native Library

The repo includes a rebuild script that fetches the pinned Adafruit Piomatter source and rebuilds the native shim directly on Linux:

```bash
./scripts/rebuild-libpi5matrix.sh
```

That script updates:

```text
runtimes/linux-arm64/native/libpi5matrix.so
```

It is best run on the Raspberry Pi 5 you intend to test with.

## Packaging

To build the NuGet package locally:

```bash
./scripts/pack.sh
```

The resulting package is written to:

```text
artifacts/nuget
```

## Releasing

The intended release flow is:

1. Build and test with `./scripts/pack.sh`
2. Create a Git tag such as `v0.1.0`
3. Publish a GitHub release and attach the generated `.nupkg`
4. Push the same package to NuGet using the `NUGET_API_KEY` repo secret

## License

This project is distributed under `GPL-2.0-only`. See [LICENSE](https://github.com/random-name-1234/Pi5MatrixSharp/blob/main/LICENSE).

The bundled native backend is built on top of Adafruit's GPL-2.0-only Pi 5 Piomatter implementation. See [THIRD_PARTY_NOTICES.md](https://github.com/random-name-1234/Pi5MatrixSharp/blob/main/THIRD_PARTY_NOTICES.md).
