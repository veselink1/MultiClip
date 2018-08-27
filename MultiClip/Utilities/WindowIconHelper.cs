using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MultiClip.Utilities
{
    /// <summary>
    /// Contains classes for managing the appearance 
    /// of the icon on the titlbar of a window.
    /// </summary>
    public static class WindowIconHelper
    {
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int index, int newStyle);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_DLGMODALFRAME = 0x0001;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_NOACTIVATE = 0x0010;
        const int SWP_FRAMECHANGED = 0x0020;
        const uint WM_SETICON = 0x0080;

        /// <summary>
        /// Hide the icon on the left side of the window titlebar.
        /// </summary>
        /// <param name="window"></param>
        public static void HideIcon(Window window)
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;

            var extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);

            SendMessage(hWnd, WM_SETICON, IntPtr.Zero, IntPtr.Zero);
            SendMessage(hWnd, WM_SETICON, new IntPtr(1), IntPtr.Zero);

            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);
        }
    }
}
