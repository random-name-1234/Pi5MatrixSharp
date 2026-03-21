using System;
using System.Runtime.InteropServices;

namespace Pi5MatrixSharp;

internal static partial class Pi5Bindings
{
    private const string Lib = "libpi5matrix.so";

    [LibraryImport(Lib, EntryPoint = "pi5_matrix_create_from_buffer")]
    public static partial IntPtr pi5_matrix_create_from_buffer(
        int colorspace,
        int pinout,
        IntPtr framebuffer,
        nuint framebufferSize,
        ref InternalPi5MatrixGeometryOptions geometry);

    [LibraryImport(Lib, EntryPoint = "pi5_matrix_delete")]
    public static partial void pi5_matrix_delete(IntPtr handle);

    [LibraryImport(Lib, EntryPoint = "pi5_matrix_show")]
    public static partial int pi5_matrix_show(IntPtr handle);

    [LibraryImport(Lib, EntryPoint = "pi5_matrix_get_fps")]
    public static partial double pi5_matrix_get_fps(IntPtr handle);

    [LibraryImport(Lib, EntryPoint = "pi5_matrix_expected_framebuffer_size")]
    public static partial nuint pi5_matrix_expected_framebuffer_size(int colorspace, int width, int height);

    [LibraryImport(Lib, EntryPoint = "pi5_matrix_last_error_message")]
    private static partial IntPtr pi5_matrix_last_error_message();

    public static string GetLastErrorMessage()
    {
        var ptr = pi5_matrix_last_error_message();
        return ptr == IntPtr.Zero
            ? "Unknown error"
            : Marshal.PtrToStringAnsi(ptr) ?? "Unknown error";
    }
}
