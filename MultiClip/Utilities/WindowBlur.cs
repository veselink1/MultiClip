using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;

namespace MultiClip.Utilities
{
    public static class WindowBlur
    {
        public static bool IsAcrylicSupported
        {
            get
            {
                if (int.TryParse(Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuild", "").ToString(), 
                    out int releaseId))
                {
                    return releaseId >= 17063;
                }
                return false;
            }
        }

        /// <summary>
        /// Enables the blur-behind effect on the window.
        /// </summary>
        /// <param name="window">The target window.</param>
        /// <returns>True if the operation succeeded.</returns>
        public static bool Enable(Window window, bool preferAcrylic = false)
        {
            return WindowComposition_SetWindowCompositionAttributeImpl.SetBlurState(window, true, preferAcrylic)
                || WindowComposition_EnableBlurBehindImpl.SetBlurState(window, true);
        }

        /// <summary>
        /// Disables the blur-behind effect on the window.
        /// </summary>
        /// <param name="window">The target window.</param>
        /// <returns>True if the operation succeeded.</returns>
        public static bool Disable(Window window)
        {
            return WindowComposition_SetWindowCompositionAttributeImpl.SetBlurState(window, false)
                || WindowComposition_EnableBlurBehindImpl.SetBlurState(window, false);
        }

        private static class WindowComposition_SetWindowCompositionAttributeImpl
        {
            [DllImport("user32.dll")]
            internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
            
            public static bool SetBlurState(Window window, bool enabled, bool preferAcrylic = false)
            {
                try
                {
                    var windowHelper = new WindowInteropHelper(window);

                    var accent = new AccentPolicy();
                    var accentStructSize = Marshal.SizeOf(accent);
                    accent.AccentState = enabled 
                        ? preferAcrylic && IsAcrylicSupported 
                            ? AccentState.ACCENT_ENABLE_ACRYLIC
                            : AccentState.ACCENT_ENABLE_BLURBEHIND 
                        : AccentState.ACCENT_DISABLED;

                    var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                    try
                    {
                        Marshal.StructureToPtr(accent, accentPtr, false);

                        var data = new WindowCompositionAttributeData
                        {
                            Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                            SizeOfData = accentStructSize,
                            Data = accentPtr
                        };

                        SetWindowCompositionAttribute(windowHelper.Handle, ref data);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(accentPtr);
                    }
                    return true;
                }
                catch (EntryPointNotFoundException)
                {
                    return false;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct WindowCompositionAttributeData
            {
                public WindowCompositionAttribute Attribute;
                public IntPtr Data;
                public int SizeOfData;
            }

            internal enum WindowCompositionAttribute
            {
                // ...
                WCA_ACCENT_POLICY = 19
                // ...
            }

            internal enum AccentState
            {
                ACCENT_DISABLED = 0,
                ACCENT_ENABLE_GRADIENT = 1,
                ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
                ACCENT_ENABLE_BLURBEHIND = 3,
                ACCENT_ENABLE_ACRYLIC = 4, // Build Ver >= 17063
                ACCENT_INVALID_STATE = 5
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct AccentPolicy
            {
                public AccentState AccentState;
                public int AccentFlags;
                public int GradientColor;
                public int AnimationId;
            }
        }

        private static class WindowComposition_EnableBlurBehindImpl
        {
            [DllImport("dwmapi.dll")]
            private static extern IntPtr DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND blurFlags);

            public static bool SetBlurState(Window window, bool enabled)
            {
                try
                {
                    WindowInteropHelper interopHelper = new WindowInteropHelper(window);
                    IntPtr hWnd = interopHelper.EnsureHandle();

                    using (HwndSource hWndSource = HwndSource.FromHwnd(hWnd))
                    {
                        hWndSource.CompositionTarget.BackgroundColor = Colors.Transparent;

                        DWM_BLURBEHIND blurBehindParameters = new DWM_BLURBEHIND
                        {
                            dwFlags = DwmBlurBehindFlags.DWM_BB_ENABLE,
                            fEnable = true,
                            hRgnBlur = IntPtr.Zero
                        };

                        return DwmEnableBlurBehindWindow(hWnd, ref blurBehindParameters).ToInt32() != 0;
                    }
                }
                catch (DllNotFoundException)
                {
                    return false;
                }
            }

            [Flags]
            private enum DwmBlurBehindFlags : uint
            {
                DWM_BB_ENABLE = 0x00000001,
                DWM_BB_BLURREGION = 0x00000002,
                DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct DWM_BLURBEHIND
            {
                public DwmBlurBehindFlags dwFlags;
                public bool fEnable;
                public IntPtr hRgnBlur;
                public bool fTansitionOnMaximized;
            }

            struct MARGINS
            {
                internal MARGINS(Thickness t)
                {
                    Left = (int)t.Left;
                    Right = (int)t.Right;
                    Top = (int)t.Top;
                    Bottom = (int)t.Bottom;
                }

                internal int Left;
                internal int Right;
                internal int Top;
                internal int Bottom;
            }
        }
    }
}
