using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Keys = System.Windows.Forms.Keys;

namespace MultiClip.Utilities
{
    public enum HotkeyModfier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        Shift = 0x0004,
        Win = 0x0008,
    }

    /// <summary>
    /// Allows the user to register a global hotkey.
    /// </summary>
    public sealed class GlobalHotkey : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        
        public event Action Pressed;

        private HotkeyModfier _modifier;
        private Keys _key;
        private IntPtr _hWnd;
        private HwndSource _hWndSource;
        private HwndSourceHook _hWndSourceHook;
        private int _id;
        private bool _isRegistered;
        private bool _isDisposed;

        public GlobalHotkey(HotkeyModfier modifier, Keys key)
        {
            _modifier = modifier;
            _key = key;
            _id = GetHashCode();
            _isRegistered = false;
            _hWnd = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
            _hWndSource = HwndSource.FromHwnd(_hWnd);
            _hWndSourceHook = WndProc;
            _hWndSource.AddHook(_hWndSourceHook);
            _isDisposed = false;
        }

        public override int GetHashCode()
        {
            return (int)_modifier ^ (int)_key ^ _hWnd.ToInt32();
        }

        /// <summary>
        /// Registers the hotkey.
        /// </summary>
        public void Register()
        {
            if (_isRegistered)
            {
                return;
            }
            if (!RegisterHotKey(_hWnd, _id, (int)_modifier, (int)_key))
            {
                throw new Exception("Couldn't register hotkey.");
            }
        }

        /// <summary>
        /// Unregisters the hotkey.
        /// </summary>
        public void Unregiser()
        {
            if (!_isRegistered)
            {
                return;
            }
            if (!UnregisterHotKey(_hWnd, _id))
            {
                throw new Exception("Couldn't unregister hotkey.");
            }
        }

        private const int WM_HOTKEY = 0x0312;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && _id == wParam.ToInt32())
            {
                Pressed?.Invoke();
                handled = false;
            }
            return IntPtr.Zero;
        }
        
        public void Dispose()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            _hWndSource.RemoveHook(WndProc);
            _hWndSource.Dispose();

            _isDisposed = true;
        }
    }
}
