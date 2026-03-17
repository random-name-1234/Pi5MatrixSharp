# Pi5MatrixSharp

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

Package page:

```text
https://www.nuget.org/packages/Pi5MatrixSharp/
```

## Quick Start

```csharp
using Pi5MatrixSharp;

var options = new Pi5MatrixOptions
{
    Pinout = Pi5MatrixPinout.AdafruitMatrixBonnet,
    Geometry = new Pi5MatrixGeometryOptions
    {
        Width = 64,
        Height = 32,
        AddressLineCount = 4
    }
};

using var matrix = new Pi5Matrix(options);

matrix.Clear();
matrix.SetPixel(0, 0, 255, 0, 0);
matrix.SetPixel(1, 0, 0, 255, 0);
matrix.SetPixel(2, 0, 0, 0, 255);
matrix.Show();
```

For a runnable example, see `samples/Pi5MatrixSharp.Sample`.

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
2. Create a Git tag such as `v0.1.0-preview.1`
3. Publish a GitHub release and attach the generated `.nupkg`
4. Push the same package to NuGet using the `NUGET_API_KEY` repo secret

## License

This project is distributed under `GPL-2.0-only`. See `LICENSE`.

The bundled native backend is built on top of Adafruit's GPL-2.0-only Pi 5 Piomatter implementation. See `THIRD_PARTY_NOTICES.md`.
