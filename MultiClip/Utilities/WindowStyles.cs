using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MultiClip.Utilities
{
    [Flags]
    public enum ExtendedWindowStyle
    {
        // ...
        WS_EX_TOPMOST =    0x00000008,
        WS_EX_TOOLWINDOW = 0x00000080,
        WS_EX_NOACTIVATE = 0x08000000,
        // ...
    }

    /// <summary>
    /// Utility class for setting the native window style.
    /// </summary>
    public static class WindowStyles
    {

        private enum GetWindowLongField
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        private static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        private static extern void SetLastError(int dwErrorCode);

        public static void SetExtended(Window window, ExtendedWindowStyle style)
        {
            IntPtr hWnd = new WindowInteropHelper(window).EnsureHandle();
            SetWindowLong(hWnd, (int)GetWindowLongField.GWL_EXSTYLE, (IntPtr)style);
        }

        public static ExtendedWindowStyle GetExtended(Window window)
        {
            IntPtr hWnd = new WindowInteropHelper(window).EnsureHandle();
            return (ExtendedWindowStyle)GetWindowLong(hWnd, (int)GetWindowLongField.GWL_EXSTYLE);
        }
    }
}
