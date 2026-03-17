#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
project_root="$(cd -- "${script_dir}/.." && pwd)"
configuration="${CONFIGURATION:-Release}"
version="${PACKAGE_VERSION:-0.1.0-preview.1}"

dotnet test "${project_root}/Pi5MatrixSharp.slnx" -c "${configuration}"
dotnet pack "${project_root}/src/Pi5MatrixSharp/Pi5MatrixSharp.csproj" \
  -c "${configuration}" \
  -o "${project_root}/artifacts/nuget" \
  -p:Version="${version}"
