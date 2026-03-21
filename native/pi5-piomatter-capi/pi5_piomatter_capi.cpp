#include "pi5_piomatter_capi.h"

#include <cerrno>
#include <cmath>
#include <cstring>
#include <memory>
#include <new>
#include <span>
#include <stdexcept>
#include <string>

#include "piomatter/piomatter.h"

static_assert(sizeof(pi5_matrix_geometry_options) == 28,
    "pi5_matrix_geometry_options must be 28 bytes to match C# InternalPi5MatrixGeometryOptions layout");

struct pi5_matrix_handle {
    std::unique_ptr<piomatter::piomatter_base> matrix;
};

namespace {
thread_local std::string g_last_error;

void clear_last_error() {
    g_last_error.clear();
}

void set_last_error(std::string message) {
    g_last_error = std::move(message);
}

piomatter::orientation to_orientation(int rotation) {
    switch (rotation) {
    case PI5_MATRIX_ORIENTATION_NORMAL:
        return piomatter::orientation::normal;
    case PI5_MATRIX_ORIENTATION_R180:
        return piomatter::orientation::r180;
    case PI5_MATRIX_ORIENTATION_CCW:
        return piomatter::orientation::ccw;
    case PI5_MATRIX_ORIENTATION_CW:
        return piomatter::orientation::cw;
    default:
        throw std::invalid_argument("rotation must be a valid pi5_matrix_orientation value");
    }
}

piomatter::matrix_geometry make_geometry(const pi5_matrix_geometry_options &options) {
    if (options.width <= 0) {
        throw std::invalid_argument("width must be positive");
    }
    if (options.height <= 0) {
        throw std::invalid_argument("height must be positive");
    }
    if (options.n_addr_lines <= 0) {
        throw std::invalid_argument("n_addr_lines must be positive");
    }
    if (options.n_planes <= 0) {
        throw std::invalid_argument("n_planes must be positive");
    }
    if (options.n_temporal_planes < 0) {
        throw std::invalid_argument("n_temporal_planes must be non-negative");
    }

    const size_t width = static_cast<size_t>(options.width);
    const size_t height = static_cast<size_t>(options.height);
    const size_t n_addr_lines = static_cast<size_t>(options.n_addr_lines);
    const size_t n_lines = 2u << n_addr_lines;
    const size_t total_pixels = width * height;
    if ((total_pixels % n_lines) != 0) {
        throw std::invalid_argument(
            "width * height must be divisible by the number of addressed row pairs");
    }

    const size_t pixels_across = total_pixels / n_lines;
    switch (to_orientation(options.rotation)) {
    case piomatter::orientation::normal:
        return piomatter::matrix_geometry(
            pixels_across,
            n_addr_lines,
            options.n_planes,
            options.n_temporal_planes,
            width,
            height,
            options.serpentine != 0,
            piomatter::orientation_normal);
    case piomatter::orientation::r180:
        return piomatter::matrix_geometry(
            pixels_across,
            n_addr_lines,
            options.n_planes,
            options.n_temporal_planes,
            width,
            height,
            options.serpentine != 0,
            piomatter::orientation_r180);
    case piomatter::orientation::ccw:
        return piomatter::matrix_geometry(
            pixels_across,
            n_addr_lines,
            options.n_planes,
            options.n_temporal_planes,
            width,
            height,
            options.serpentine != 0,
            piomatter::orientation_ccw);
    case piomatter::orientation::cw:
        return piomatter::matrix_geometry(
            pixels_across,
            n_addr_lines,
            options.n_planes,
            options.n_temporal_planes,
            width,
            height,
            options.serpentine != 0,
            piomatter::orientation_cw);
    }

    throw std::invalid_argument("rotation must be a valid pi5_matrix_orientation value");
}

template <typename TColorspace>
std::span<const typename TColorspace::data_type> make_framebuffer_span(
    const void *framebuffer,
    size_t framebuffer_size,
    size_t n_pixels) {
    if (framebuffer == nullptr) {
        throw std::invalid_argument("framebuffer must not be null");
    }

    const size_t expected_size = TColorspace::data_size_in_bytes(n_pixels);
    if (framebuffer_size != expected_size) {
        throw std::invalid_argument("framebuffer size does not match the selected colorspace");
    }

    using data_type = typename TColorspace::data_type;
    const auto *data = static_cast<const data_type *>(framebuffer);
    return std::span<const data_type>(data, expected_size / sizeof(data_type));
}

template <typename TPinout, typename TColorspace>
std::unique_ptr<piomatter::piomatter_base> make_matrix_instance(
    const void *framebuffer,
    size_t framebuffer_size,
    const piomatter::matrix_geometry &geometry) {
    auto span = make_framebuffer_span<TColorspace>(
        framebuffer,
        framebuffer_size,
        geometry.width * geometry.height);
    return std::make_unique<piomatter::piomatter<TPinout, TColorspace>>(span, geometry);
}

template <typename TPinout>
std::unique_ptr<piomatter::piomatter_base> make_matrix_for_pinout(
    int colorspace,
    const void *framebuffer,
    size_t framebuffer_size,
    const piomatter::matrix_geometry &geometry) {
    switch (colorspace) {
    case PI5_MATRIX_COLORSPACE_RGB565:
        return make_matrix_instance<TPinout, piomatter::colorspace_rgb565>(
            framebuffer,
            framebuffer_size,
            geometry);
    case PI5_MATRIX_COLORSPACE_RGB888:
        return make_matrix_instance<TPinout, piomatter::colorspace_rgb888>(
            framebuffer,
            framebuffer_size,
            geometry);
    case PI5_MATRIX_COLORSPACE_RGB888_PACKED:
        return make_matrix_instance<TPinout, piomatter::colorspace_rgb888_packed>(
            framebuffer,
            framebuffer_size,
            geometry);
    default:
        throw std::invalid_argument("colorspace must be a valid pi5_matrix_colorspace value");
    }
}

std::unique_ptr<piomatter::piomatter_base> make_matrix(
    int colorspace,
    int pinout,
    const void *framebuffer,
    size_t framebuffer_size,
    const piomatter::matrix_geometry &geometry) {
    switch (pinout) {
    case PI5_MATRIX_PINOUT_ADAFRUIT_MATRIX_BONNET:
        return make_matrix_for_pinout<piomatter::adafruit_matrix_bonnet_pinout>(
            colorspace,
            framebuffer,
            framebuffer_size,
            geometry);
    case PI5_MATRIX_PINOUT_ADAFRUIT_MATRIX_BONNET_BGR:
        return make_matrix_for_pinout<piomatter::adafruit_matrix_bonnet_pinout_bgr>(
            colorspace,
            framebuffer,
            framebuffer_size,
            geometry);
    case PI5_MATRIX_PINOUT_ACTIVE3:
        return make_matrix_for_pinout<piomatter::active3_pinout>(
            colorspace,
            framebuffer,
            framebuffer_size,
            geometry);
    case PI5_MATRIX_PINOUT_ACTIVE3_BGR:
        return make_matrix_for_pinout<piomatter::active3_pinout_bgr>(
            colorspace,
            framebuffer,
            framebuffer_size,
            geometry);
    default:
        throw std::invalid_argument("pinout must be a valid pi5_matrix_pinout value");
    }
}
} // namespace

extern "C" pi5_matrix_handle *pi5_matrix_create_from_buffer(
    int colorspace,
    int pinout,
    const void *framebuffer,
    size_t framebuffer_size,
    const pi5_matrix_geometry_options *geometry) {
    if (geometry == nullptr) {
        set_last_error("geometry must not be null");
        return nullptr;
    }

    try {
        auto matrix_geometry = make_geometry(*geometry);
        auto matrix = make_matrix(colorspace, pinout, framebuffer, framebuffer_size, matrix_geometry);
        auto *handle = new (std::nothrow) pi5_matrix_handle{std::move(matrix)};
        if (handle == nullptr) {
            set_last_error("failed to allocate pi5_matrix_handle");
            return nullptr;
        }

        clear_last_error();
        return handle;
    } catch (const std::exception &ex) {
        set_last_error(ex.what());
        return nullptr;
    }
}

extern "C" void pi5_matrix_delete(pi5_matrix_handle *handle) {
    delete handle;
}

extern "C" int pi5_matrix_show(pi5_matrix_handle *handle) {
    if (handle == nullptr) {
        set_last_error("handle must not be null");
        return EINVAL;
    }

    const int err = handle->matrix->show();
    if (err != 0) {
        set_last_error(std::strerror(err));
        return err;
    }

    clear_last_error();
    return 0;
}

extern "C" double pi5_matrix_get_fps(const pi5_matrix_handle *handle) {
    if (handle == nullptr) {
        return 0.0;
    }

    return handle->matrix->fps;
}

extern "C" size_t pi5_matrix_expected_framebuffer_size(int colorspace, int width, int height) {
    if (width <= 0 || height <= 0) {
        return 0;
    }

    const size_t pixel_count = static_cast<size_t>(width) * static_cast<size_t>(height);
    switch (colorspace) {
    case PI5_MATRIX_COLORSPACE_RGB565:
        return piomatter::colorspace_rgb565::data_size_in_bytes(pixel_count);
    case PI5_MATRIX_COLORSPACE_RGB888:
        return piomatter::colorspace_rgb888::data_size_in_bytes(pixel_count);
    case PI5_MATRIX_COLORSPACE_RGB888_PACKED:
        return piomatter::colorspace_rgb888_packed::data_size_in_bytes(pixel_count);
    default:
        return 0;
    }
}

extern "C" const char *pi5_matrix_last_error_message(void) {
    return g_last_error.c_str();
}
