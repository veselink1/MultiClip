using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Threading;
using MultiClip.Native;

namespace MultiClip.Utilities
{
    public class MouseHookEventArgs : EventArgs
    {
        /// <summary>
        /// Specifies the screen-space position of the mouse.
        /// </summary>
        public Point Point { get; set; }
        /// <summary>
        /// Specifies whether the event will be handled.
        /// A handled event will not propagate to other applications.
        /// </summary>
        public bool Handled { get; set; }

        public MouseHookEventArgs(Point pt)
        {
            Point = pt;
            Handled = false;
        }
    }

    /// <summary>
    /// Allows the user to register a global keyboard hook.
    /// </summary>
    public sealed class LowLevelMouseHook : IDisposable
    {
        public event EventHandler<MouseHookEventArgs> MouseMove;
        public event EventHandler<MouseHookEventArgs> LButtonDown;
        public event EventHandler<MouseHookEventArgs> LButtonUp;
        public event EventHandler<MouseHookEventArgs> LButtonDblClick;
        public event EventHandler<MouseHookEventArgs> RButtonDown;
        public event EventHandler<MouseHookEventArgs> RButtonUp;
        public event EventHandler<MouseHookEventArgs> RButtonDblClick;
        public event EventHandler<MouseHookEventArgs> MButtonDown;
        public event EventHandler<MouseHookEventArgs> MButtonUp;
        public event EventHandler<MouseHookEventArgs> MButtonDblClick;
        public event EventHandler<MouseHookEventArgs> XButtonDown;
        public event EventHandler<MouseHookEventArgs> XButtonUp;
        public event EventHandler<MouseHookEventArgs> XButtonDblClick;
        public event EventHandler<MouseHookEventArgs> MouseVWheel;
        public event EventHandler<MouseHookEventArgs> MouseHWheel;

        public bool IsHookSet => _nativeHookId != IntPtr.Zero;

        private NativeMethods.HookProc _hookProc;
        private IntPtr _nativeHookId;
        private bool _isDisposed;

        public LowLevelMouseHook()
        {
            _hookProc = HookCallback;
            _nativeHookId = IntPtr.Zero;
            _isDisposed = false;
        }

        /// <summary>
        /// Register the mouse hook.
        /// </summary>
        public void SetHook()
        {
            if (IsHookSet)
            {
                throw new Exception("The low-level mouse hook is already set.");
            }

            using (Process process = Process.GetCurrentProcess())
            using (ProcessModule module = process.MainModule)
            {
                _nativeHookId = NativeMethods.SetWindowsHookEx(
                    NativeMethods.WH_MOUSE_LL,
                    _hookProc,
                    NativeMethods.GetModuleHandle(module.ModuleName),
                    0);

                if (_nativeHookId == IntPtr.Zero)
                {
                    throw new Exception("Couldn't set the low-level mouse hook.");
                }
            }
        }

        /// <summary>
        /// Unregister the mouse hook.
        /// </summary>
        public void RemoveHook()
        {
            if (!IsHookSet)
            {
                throw new Exception("The low-level mouse hook is not set.");
            }

            if (!NativeMethods.UnhookWindowsHookEx(_nativeHookId))
            {
                throw new Exception("Couldn't remove the low-level mouse hook.");
            }
            _nativeHookId = IntPtr.Zero;
        }

        /// <summary>
        /// The internal hook callback.
        /// </summary>
        private IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            int nmsg = wParam.ToInt32();
            WindowMessage msg = (WindowMessage)nmsg;

            if (nCode >= 0 &&
                (nmsg >= (int)WindowMessage.WM_MOUSEFIRST && nmsg <= (int)WindowMessage.WM_MOUSEWHEEL))
            {
                try
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    Keys key = (Keys)vkCode;

                    if (!NativeMethods.GetCursorPos(out NativeMethods.POINT pt))
                    {
                        throw new Win32Exception();
                    }

                    MouseHookEventArgs eventArgs = new MouseHookEventArgs(new Point(pt.X, pt.Y));

                    switch (msg)
                    {
                        case WindowMessage.WM_MOUSEMOVE:
                            MouseMove?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_LBUTTONDOWN:
                            LButtonDown?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_LBUTTONUP:
                            LButtonUp?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_LBUTTONDBLCLK:
                            LButtonDblClick?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_RBUTTONDOWN:
                            RButtonDown?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_RBUTTONUP:
                            RButtonUp?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_RBUTTONDBLCLK:
                            RButtonDblClick?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_MBUTTONDOWN:
                            MButtonDown?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_MBUTTONUP:
                            MButtonUp?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_MBUTTONDBLCLK:
                            MButtonDblClick?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_XBUTTONDOWN:
                            XButtonDown?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_XBUTTONUP:
                            XButtonUp?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_XBUTTONDBLCLK:
                            XButtonDblClick?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_MOUSEWHEEL:
                            MouseVWheel?.Invoke(this, eventArgs);
                            break;
                        case WindowMessage.WM_MOUSEHWHEEL:
                            MouseHWheel?.Invoke(this, eventArgs);
                            break;
                    }

                    // If Handled is set to true, 
                    // return 1 and stop the keyboard event
                    // from propagating to other applications.
                    if (eventArgs.Handled)
                    {
                        return (IntPtr)1;
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        System.Windows.MessageBox.Show(e.ToString(), "Application Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    });
                }
            }
            return NativeMethods.CallNextHookEx(_nativeHookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            if (IsHookSet)
            {
                RemoveHook();
            }

            _isDisposed = true;
        }
    }
}
