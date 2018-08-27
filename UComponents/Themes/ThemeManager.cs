using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using UComponents.Utilities;

namespace UComponents.Themes
{
    public enum Theme
    {
        Light,
        Dark,
    }

    public static class ThemeManager
    {
        private static Color s_accentColor;
        private static Theme? s_theme;

        public static Theme CurrentTheme
        {
            get
            {
                if (s_theme != null)
                {
                    return s_theme.Value;
                }
                else
                {
                    throw new InvalidOperationException("Cannot get current theme name: No theme was previously applied.");
                }
            }
        }

        static ThemeManager()
        {
            SystemAccentColor.ColorChanged += HandleAccentColorChanged;
            s_accentColor = SystemAccentColor.GetColor();
            ForceUpdateColorization();
        }

        public static void Reapply()
        {
            if (s_theme != null)
            {
                Apply(s_theme.Value);
            }
            else
            {
                throw new InvalidOperationException("Cannot reapply theme: No theme was previously applied.");
            }
        }

        public static void Apply(Theme themeVariant)
        {
            foreach (var window in Application.Current.Windows.Cast<System.Windows.Window>())
            {
                s_theme = themeVariant;
                ApplyThemeResources(window, themeVariant);
            }
        }

        private static void ForceUpdateColorization()
        {
            foreach (var window in Application.Current.Windows.Cast<System.Windows.Window>())
            {
                foreach (var namedSwitch in Colors.AllNamed.Where((Tuple<string, SolidColorSwitch> x) => x.Item2.UseAccentForRGB))
                {
                    string resourceName = namedSwitch.Item1;
                    SolidColorSwitch colorSwitch = namedSwitch.Item2;
                    window.Resources[resourceName] =
                        s_theme == Theme.Light
                            ? new SolidColorBrush(ColorizeRGBWithAccent(colorSwitch.LightColorBrush.Color))
                            : new SolidColorBrush(ColorizeRGBWithAccent(colorSwitch.DarkColorBrush.Color));
                }
            }
        }

        private static void HandleAccentColorChanged(Color newColor)
        {
            s_accentColor = newColor;
            ForceUpdateColorization();
        }

        private static Color ColorizeRGBWithAccent(Color color)
        {
            return Color.FromArgb(color.A, s_accentColor.R, s_accentColor.G, s_accentColor.B);
        }

        private static void ApplyThemeResources(System.Windows.Window window, Theme theme)
        {
            foreach (var namedSwitch in Colors.AllNamed)
            {
                string resourceName = namedSwitch.Item1;
                SolidColorSwitch colorSwitch = namedSwitch.Item2;
                if (colorSwitch.UseAccentForRGB)
                {
                    window.Resources[resourceName] =
                        theme == Theme.Light
                            ? new SolidColorBrush(ColorizeRGBWithAccent(colorSwitch.LightColorBrush.Color))
                            : new SolidColorBrush(ColorizeRGBWithAccent(colorSwitch.DarkColorBrush.Color));
                }
                else
                {
                    window.Resources[resourceName] =
                        theme == Theme.Light
                            ? colorSwitch.LightColorBrush
                            : colorSwitch.DarkColorBrush;
                }
            }
        }

        public static SolidColorBrush CreateAnimatedBrush(SolidColorBrush from, SolidColorBrush to, TimeSpan timeSpan)
        {
            SolidColorBrush brush = new SolidColorBrush
            {
                Color = from.Color,
                Opacity = from.Opacity,
            };

            ColorAnimation colorAnim = new ColorAnimation(to.Color, timeSpan);
            DoubleAnimation opacityAnim = new DoubleAnimation(to.Opacity, timeSpan);

            brush.BeginAnimation(Brush.OpacityProperty, opacityAnim);
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            return brush;
        }
    }
}
