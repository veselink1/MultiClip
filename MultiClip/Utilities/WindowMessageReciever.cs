using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using MultiClip.Native;

namespace MultiClip.Utilities
{
    /// <summary>
    /// Creates a hidden window instance and allows the user 
    /// to handle messages to this instance. 
    /// </summary>
    public class WindowMessageReceiver : IDisposable
    {
        public delegate void MessageHandler(int msg, IntPtr wParam, IntPtr lParam);
        public event MessageHandler MessageReceived;
        public bool IsOpened => _hWnd != IntPtr.Zero;

        private Dispatcher _dispatcher;
        private string _className;
        private string _windowName;
        private IntPtr _hWnd;
        private Thread _mainThread;
        private bool _isDisposed;

        public WindowMessageReceiver(string className, string windowName)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _className = className;
            _windowName = windowName;
            _hWnd = IntPtr.Zero;
            _mainThread = new Thread(WndThread);
            _mainThread.Start();
            _isDisposed = false;
        }

        public void Close()
        {
            _mainThread.Abort();
            _hWnd = IntPtr.Zero;
            _mainThread = null;
        }

        private void WndThread()
        {
            IntPtr hInstance = Marshal.GetHINSTANCE(Application.Current.GetType().Module);
            var wndClass = new NativeMethods.WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.WNDCLASSEX)),
                lpszClassName = _className,
                lpfnWndProc = WndProc,
                hInstance = hInstance,
            };

            NativeMethods.RegisterClassEx(ref wndClass);

            IntPtr _hWnd = NativeMethods.CreateWindowEx(0, _className, _windowName, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);
            if (_hWnd == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            NativeMethods.MSG msg = default;
            while (NativeMethods.GetMessage(out msg, _hWnd, 0, 0) != 0)
            {
                NativeMethods.TranslateMessage(ref msg);
                NativeMethods.DispatchMessage(ref msg);
            }
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if ((int)msg >= (int)WindowMessage.WM_APP && (int)msg <= 0xBFFF)
            {
                _dispatcher.BeginInvoke(new Action(() => 
                {
                    MessageReceived?.Invoke((int)msg, wParam, lParam);
                }));
            }

            return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            if (IsOpened)
            {
                Close();
            }

            _isDisposed = true;
        }
    }
}
