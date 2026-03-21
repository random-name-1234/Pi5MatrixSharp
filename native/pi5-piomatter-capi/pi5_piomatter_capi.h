#pragma once

#include <stddef.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef enum pi5_matrix_colorspace {
    PI5_MATRIX_COLORSPACE_RGB565 = 0,
    PI5_MATRIX_COLORSPACE_RGB888 = 1,
    PI5_MATRIX_COLORSPACE_RGB888_PACKED = 2,
} pi5_matrix_colorspace;

typedef enum pi5_matrix_pinout {
    PI5_MATRIX_PINOUT_ADAFRUIT_MATRIX_BONNET = 0,
    PI5_MATRIX_PINOUT_ADAFRUIT_MATRIX_BONNET_BGR = 1,
    PI5_MATRIX_PINOUT_ACTIVE3 = 2,
    PI5_MATRIX_PINOUT_ACTIVE3_BGR = 3,
} pi5_matrix_pinout;

typedef enum pi5_matrix_orientation {
    PI5_MATRIX_ORIENTATION_NORMAL = 0,
    PI5_MATRIX_ORIENTATION_R180 = 1,
    PI5_MATRIX_ORIENTATION_CCW = 2,
    PI5_MATRIX_ORIENTATION_CW = 3,
} pi5_matrix_orientation;

typedef struct pi5_matrix_geometry_options {
    int width;
    int height;
    int n_addr_lines;
    unsigned char serpentine;
    unsigned char reserved0;
    unsigned char reserved1;
    unsigned char reserved2;
    int rotation;
    int n_planes;
    int n_temporal_planes;
} pi5_matrix_geometry_options;

typedef struct pi5_matrix_handle pi5_matrix_handle;

pi5_matrix_handle *pi5_matrix_create_from_buffer(
    int colorspace,
    int pinout,
    const void *framebuffer,
    size_t framebuffer_size,
    const pi5_matrix_geometry_options *geometry);

void pi5_matrix_delete(pi5_matrix_handle *handle);

int pi5_matrix_show(pi5_matrix_handle *handle);

double pi5_matrix_get_fps(const pi5_matrix_handle *handle);

size_t pi5_matrix_expected_framebuffer_size(int colorspace, int width, int height);

const char *pi5_matrix_last_error_message(void);

#ifdef __cplusplus
}
#endif
