using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using MultiClip.Native;

namespace MultiClip.Utilities
{
    public class KeyboardHookEventArgs : EventArgs
    {
        /// <summary>
        /// The keys resulting in the event.
        /// </summary>
        public Keys Keys { get; private set; }
        /// <summary>
        /// Specifies whether the event will be handled.
        /// A handled event will not propagate to other applications.
        /// </summary>
        public bool Handled { get; set; }

        public KeyboardHookEventArgs(Keys keys)
        {
            Keys = keys;
            Handled = false;
        }
    }

    /// <summary>
    /// Allows the user to register a global keyboard hook.
    /// </summary>
    public sealed class LowLevelKeyboardHook : IDisposable
    {
        /// <summary>
        /// Occurs when the user presses a key down.
        /// </summary>
        public event EventHandler<KeyboardHookEventArgs> KeyDown;
        /// <summary>
        /// Occurs when the user releases a key.
        /// </summary>
        public event EventHandler<KeyboardHookEventArgs> KeyUp;

        /// <summary>
        /// Is the control key held currently.
        /// </summary>
        public bool IsCtrlHeld { get; private set; } = false;

        public bool IsHookSet => _nativeHookId != IntPtr.Zero;

        private NativeMethods.HookProc _hookProc;
        private IntPtr _nativeHookId;

        public LowLevelKeyboardHook()
        {
            _hookProc = HookCallback;
            _nativeHookId = IntPtr.Zero;
        }

        /// <summary>
        /// Register the keyboard hook.
        /// </summary>
        public void SetHook()
        {
            if (_nativeHookId != IntPtr.Zero)
            {
                throw new Exception("The low-level keyboard hook is already set.");
            }

            using (Process process = Process.GetCurrentProcess())
            using (ProcessModule module = process.MainModule)
            {
                _nativeHookId = NativeMethods.SetWindowsHookEx(
                    NativeMethods.WH_KEYBOARD_LL,
                    _hookProc,
                    NativeMethods.GetModuleHandle(module.ModuleName), 
                    0);

                if (_nativeHookId == IntPtr.Zero)
                {
                    throw new Exception("Couldn't set the low-level keyboard hook.");
                }
            }
        }

        /// <summary>
        /// Unregister the keyboard hook.
        /// </summary>
        public void RemoveHook()
        {
            if (_nativeHookId == IntPtr.Zero)
            {
                throw new Exception("The low-level keyboard hook is not set.");
            }

            if (!NativeMethods.UnhookWindowsHookEx(_nativeHookId))
            {
                throw new Exception("Couldn't remove the low-level keyboard hook.");
            }
            _nativeHookId = IntPtr.Zero;
        }

        /// <summary>
        /// The internal hook callback.
        /// </summary>
        private IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && 
                (wParam.ToInt32() == (int)WindowMessage.WM_KEYDOWN || wParam.ToInt32() == (int)WindowMessage.WM_KEYDOWN))
            {
                try
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    Keys key = (Keys)vkCode;
                    KeyboardHookEventArgs eventArgs = new KeyboardHookEventArgs(key);
                    if (wParam.ToInt32() == (int)WindowMessage.WM_KEYDOWN)
                    {
                        if (key == Keys.LControlKey || key == Keys.RControlKey)
                        {
                            IsCtrlHeld = true;
                        }
                        else
                        {
                            KeyDown?.Invoke(this, eventArgs);
                        }
                    }
                    else if (wParam.ToInt32() == (int)WindowMessage.WM_KEYUP)
                    {
                        if (key == Keys.LControlKey || key == Keys.RControlKey)
                        {
                            IsCtrlHeld = false;
                        }
                        else
                        {
                            KeyUp?.Invoke(this, eventArgs);
                        }
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
                        System.Windows.MessageBox.Show(e.ToString(), "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            }
            return NativeMethods.CallNextHookEx(_nativeHookId, nCode, wParam, lParam);
        }
        
        public void Dispose()
        {
            if (_nativeHookId != IntPtr.Zero)
            {
                RemoveHook();
            }
        }
    }
}
