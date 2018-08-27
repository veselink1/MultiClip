using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace UComponents.Utilities
{
    public static class SystemAccentColor
    {
        public static event Action<Color> ColorChanged
        {
            add
            {
                EnsureValidState();
                s_dwmColorizationHelper.DwmColorizationColorChanged += value;
            }
            remove
            {
                EnsureValidState();
                s_dwmColorizationHelper.DwmColorizationColorChanged -= value;
            }
        }

        private static readonly object s_initSyncRoot = new object();
        private static bool s_stateSyncRoot = false;
        private static DwmColorizationHelper s_dwmColorizationHelper = null;
        private static System.Windows.Window s_lastMainWindow = null;

        private static void EnsureValidState()
        {
            lock (s_initSyncRoot)
            {
                if (!s_stateSyncRoot)
                {
                    s_lastMainWindow = Application.Current.MainWindow;
                    s_dwmColorizationHelper = new DwmColorizationHelper(s_lastMainWindow);
                }
                else if (s_lastMainWindow != Application.Current.MainWindow)
                {
                    s_dwmColorizationHelper.Dispose();
                    s_lastMainWindow = Application.Current.MainWindow;
                    s_dwmColorizationHelper = new DwmColorizationHelper(s_lastMainWindow);
                }
            }
        }

        public static Color GetColor()
        {
            EnsureValidState();
            return DwmColorizationHelper.GetWindowColorizationColor();
        }

        private class DwmColorizationHelper : IDisposable
        {
            [DllImport("dwmapi.dll", EntryPoint = "#127")]
            private static extern void DwmGetColorizationParameters(ref DWMCOLORIZATIONPARAMS dwmColorParams);

            [StructLayout(LayoutKind.Sequential)]
            public struct DWMCOLORIZATIONPARAMS
            {
                public uint ColorizationColor,
                    ColorizationAfterglow,
                    ColorizationColorBalance,
                    ColorizationAfterglowBalance,
                    ColorizationBlurBalance,
                    ColorizationGlassReflectionIntensity,
                    ColorizationOpaqueBlend;
            }

            private const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x320;

            public event Action<Color> DwmColorizationColorChanged;
            private System.Windows.Window _window;
            private IntPtr _hWnd;
            private HwndSource _hWndSource;

            public DwmColorizationHelper(System.Windows.Window window)
            {
                _window = window;
                _hWnd = new WindowInteropHelper(window).EnsureHandle();
                _hWndSource = HwndSource.FromHwnd(_hWnd);
                _hWndSource.AddHook(WndProc);
            }

            public void Dispose()
            {
                _hWndSource.RemoveHook(WndProc);
            }

            public static Color GetWindowColorizationColor()
            {
                DWMCOLORIZATIONPARAMS dwmColorParams = default(DWMCOLORIZATIONPARAMS);
                DwmGetColorizationParameters(ref dwmColorParams);

                return Color.FromArgb(
                    (byte)(dwmColorParams.ColorizationColor >> 24),
                    (byte)(dwmColorParams.ColorizationColor >> 16),
                    (byte)(dwmColorParams.ColorizationColor >> 8),
                    (byte)(dwmColorParams.ColorizationColor)
                );
            }

            private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                if (msg == WM_DWMCOLORIZATIONCOLORCHANGED)
                {
                    handled = true;
                    try
                    {
                        DwmColorizationColorChanged?.Invoke(GetWindowColorizationColor());
                    }
                    catch
                    {
                        // Ignore all exceptions.
                    }
                }
                handled = false;
                return IntPtr.Zero;
            }
        }
    }
}
