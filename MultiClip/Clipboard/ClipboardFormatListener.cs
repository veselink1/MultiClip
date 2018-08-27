using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using MultiClip.Native;

namespace MultiClip.Clipboard
{
    /// <summary>
    /// Allows the user to listen to changes to the system clipboard.
    /// </summary>
    public sealed class ClipboardFormatListener : IDisposable
    {
        public event Action ClipboardUpdated;

        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private readonly IntPtr _hWnd;
        private readonly HwndSource _hWndSource;

        /// <summary>
        /// Create a ClipboardFormatListener.
        /// </summary>
        /// <param name="owner">The owner window.</param>
        public ClipboardFormatListener(Window owner)
        {
            _hWnd = new WindowInteropHelper(owner).EnsureHandle();
            NativeMethods.AddClipboardFormatListener(_hWnd);

            _hWndSource = HwndSource.FromHwnd(_hWnd);
            _hWndSource.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                handled = true;
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ClipboardUpdated?.Invoke();
                });
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            _hWndSource.Dispose();
        }
    }
}
