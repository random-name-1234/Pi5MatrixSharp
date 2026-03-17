#!/usr/bin/env bash
set -euo pipefail

repo_url="${PI5_PIOMATTER_REPO_URL:-https://github.com/adafruit/Adafruit_Blinka_Raspberry_Pi5_Piomatter.git}"
repo_ref="${PI5_PIOMATTER_REF:-9ce4965a3fddf5b44c9da6c8dc3738cfe0403028}"

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
project_root="$(cd -- "${script_dir}/.." && pwd)"
native_root="${project_root}/native/pi5-piomatter-capi"
native_output="${project_root}/runtimes/linux-arm64/native/libpi5matrix.so"

if [[ "$(uname -s)" != "Linux" ]]; then
  echo "This script must be run on Linux (ideally directly on the Raspberry Pi 5)." >&2
  exit 1
fi

for cmd in git cc c++; do
  if ! command -v "${cmd}" >/dev/null 2>&1; then
    echo "Missing required command: ${cmd}" >&2
    exit 1
  fi
done

tmp_dir="$(mktemp -d /tmp/pi5-piomatter-XXXXXX)"
cleanup() {
  rm -rf "${tmp_dir}"
}
trap cleanup EXIT

echo "Cloning ${repo_url} (${repo_ref}) ..."
git init "${tmp_dir}/src" >/dev/null
git -C "${tmp_dir}/src" remote add origin "${repo_url}"
git -C "${tmp_dir}/src" fetch --depth 1 origin "${repo_ref}" >/dev/null
git -C "${tmp_dir}/src" checkout --detach FETCH_HEAD >/dev/null

build_dir="${tmp_dir}/build"
mkdir -p "${build_dir}"
mkdir -p "$(dirname "${native_output}")"

include_args=(
  "-I${native_root}"
  "-I${tmp_dir}/src/src/include"
  "-I${tmp_dir}/src/src/piolib/include"
)

echo "Compiling piolib C sources ..."
cc -std=gnu11 -D_GNU_SOURCE -D_POSIX_C_SOURCE=199309L -fPIC -O2 -g "${include_args[@]}" \
  -c "${tmp_dir}/src/src/piolib/piolib.c" \
  -o "${build_dir}/piolib.o"

cc -std=gnu11 -D_GNU_SOURCE -D_POSIX_C_SOURCE=199309L -fPIC -O2 -g "${include_args[@]}" \
  -c "${tmp_dir}/src/src/piolib/pio_rp1.c" \
  -o "${build_dir}/pio_rp1.o"

echo "Compiling Pi 5 matrix shim ..."
c++ -std=c++20 -fPIC -O2 -g "${include_args[@]}" \
  -c "${native_root}/pi5_piomatter_capi.cpp" \
  -o "${build_dir}/pi5_piomatter_capi.o"

echo "Linking libpi5matrix.so ..."
c++ -shared \
  -Wl,-soname,libpi5matrix.so \
  -o "${native_output}" \
  "${build_dir}/piolib.o" \
  "${build_dir}/pio_rp1.o" \
  "${build_dir}/pi5_piomatter_capi.o" \
  -lpthread

echo "Updated ${native_output}"
