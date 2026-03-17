using System;
using System.Runtime.InteropServices;

namespace Pi5MatrixSharp;

internal static class Pi5Bindings
{
    private const string Lib = "libpi5matrix.so";

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr pi5_matrix_create_from_buffer(
        int colorspace,
        int pinout,
        IntPtr framebuffer,
        nuint framebufferSize,
        ref InternalPi5MatrixGeometryOptions geometry);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void pi5_matrix_delete(IntPtr handle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern int pi5_matrix_show(IntPtr handle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern double pi5_matrix_get_fps(IntPtr handle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern nuint pi5_matrix_expected_framebuffer_size(int colorspace, int width, int height);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr pi5_matrix_last_error_message();

    public static string GetLastErrorMessage()
    {
        var ptr = pi5_matrix_last_error_message();
        return ptr == IntPtr.Zero
            ? "Unknown error"
            : Marshal.PtrToStringAnsi(ptr) ?? "Unknown error";
    }
}
