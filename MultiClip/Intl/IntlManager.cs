using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace MultiClip.Intl
{
    public static class IntlManager
    {
        public static CultureInfo DefaultCulture = new CultureInfo("en-US");

        private static CultureInfo s_cultureInfo = DefaultCulture;

        private static readonly List<WeakReference<Window>> s_registeredWindows = new List<WeakReference<Window>>();

        public static CultureInfo CurrentCulture
        {
            get
            {
                if (s_cultureInfo != null)
                {
                    return s_cultureInfo;
                }
                else
                {
                    throw new InvalidOperationException("Cannot get current theme name: No theme was previously applied.");
                }
            }
        }

        public static void Reapply()
        {
            if (s_cultureInfo != null)
            {
                Apply(s_cultureInfo);
            }
            else
            {
                throw new InvalidOperationException("Cannot reapply culture info: No culture info was previously applied.");
            }
        }

        public static void Apply(CultureInfo cultureInfo)
        {
            foreach (var window in Application.Current.Windows.Cast<Window>())
            {
                s_cultureInfo = cultureInfo;
                ApplyIntlResources(window, cultureInfo);
            }
        }

        private static void ForceUpdateResources()
        {
            foreach (var window in Application.Current.Windows.Cast<Window>())
            {
                ApplyIntlResources(window, s_cultureInfo);
            }
        }

        private static void ApplyIntlResources(Window window, CultureInfo cultureInfo)
        {
            if (s_registeredWindows.FirstOrDefault(x => x.TryGetTarget(out var other) && ReferenceEquals(other, window)) == null)
            {
                IEnumerable<string> keys = window.Resources.Keys.Cast<object>().Select(x => x as string).Where(x => x != null);
                foreach (var key in keys
                    .Where(x => !x.Contains("!") && keys.Any(y => y.StartsWith(x))))
                {
                    window.Resources[key + "!" + DefaultCulture.Name] = window.Resources[key];
                }
                s_registeredWindows.Add(new WeakReference<Window>(window));
            }

            foreach (var key in window.Resources.Keys.Cast<object>()
                .Select(x => x as string)
                .Where(x => x != null && x.Contains("!")))
            {
                string[] parts = key.Split('!');
                if (parts.Length == 2 && parts[1] == cultureInfo.Name)
                {
                    window.Resources[parts[0]] = window.Resources[key];
                }
            }
        }
    }
}
