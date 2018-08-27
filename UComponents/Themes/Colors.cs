using System;
using System.Linq;
using System.Windows.Media;

namespace UComponents.Themes
{
    public static class Colors
    {
        private static SolidColorBrush FromHex(uint value)
        {
            return new SolidColorBrush(Color.FromArgb(
                (byte)((value & 0xFF000000) >> 24),
                (byte)((value & 0x00FF0000) >> 16),
                (byte)((value & 0x0000FF00) >> 8),
                (byte)(value & 0x000000FF)
            ));
        }

        private static Tuple<string, SolidColorSwitch>[] s_allNamed;

        /*** All SolidColorSwitches ***/
        public static Tuple<string, SolidColorSwitch>[] AllNamed => s_allNamed;

        static Colors()
        {
            s_allNamed = typeof(Colors)
                .GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                .Where(prop => prop.PropertyType == typeof(SolidColorSwitch))
                .Select(prop => Tuple.Create(prop.Name, (SolidColorSwitch)prop.GetValue(null)))
                .ToArray();
        }

        /*** Base ***/
        public static SolidColorSwitch UColorBaseHighlight => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x16000000),
            DarkColorBrush = FromHex(0x16FFFFFF),
        };

        public static SolidColorSwitch UColorBaseLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x33000000),
            DarkColorBrush = FromHex(0x33FFFFFF),
        };

        public static SolidColorSwitch UColorBaseMediumLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x66000000),
            DarkColorBrush = FromHex(0x66FFFFFF),
        };
        
        public static SolidColorSwitch UColorBaseMedium => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x99000000),
            DarkColorBrush = FromHex(0x99FFFFFF),
        };

        public static SolidColorSwitch UColorBaseMediumHigh => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xCC000000),
            DarkColorBrush = FromHex(0xCCFFFFFF),
        };

        public static SolidColorSwitch UColorBaseHigh => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFF000000),
            DarkColorBrush = FromHex(0xFFFFFFFF),
        };

        /*** Alt ***/
        public static SolidColorSwitch UColorAltLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x33FFFFFF),
            DarkColorBrush = FromHex(0x33000000),
        };

        public static SolidColorSwitch UColorAltMediumLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x66FFFFFF),
            DarkColorBrush = FromHex(0x66000000),
        };

        public static SolidColorSwitch UColorAltMedium => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x99FFFFFF),
            DarkColorBrush = FromHex(0x99000000),
        };

        public static SolidColorSwitch UColorAltMediumHigh => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xCCFFFFFF),
            DarkColorBrush = FromHex(0xCC000000),
        };

        public static SolidColorSwitch UColorAltHigh => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFFFFFFFF),
            DarkColorBrush = FromHex(0xFF000000),
        };

        /*** List ***/
        public static SolidColorSwitch UColorListLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x19000000),
            DarkColorBrush = FromHex(0x19FFFFFF),
        };

        public static SolidColorSwitch UColorListMedium => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x33000000),
            DarkColorBrush = FromHex(0x33FFFFFF),
        };

        public static SolidColorSwitch UColorListAccentLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x660078D7),
            DarkColorBrush = FromHex(0x990078D7),
            UseAccentForRGB = true,
        };

        public static SolidColorSwitch UColorListAccentMedium => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x990078D7),
            DarkColorBrush = FromHex(0xCC0078D7),
            UseAccentForRGB = true,
        };

        public static SolidColorSwitch UColorListAccentHigh => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xB2000000),
            DarkColorBrush = FromHex(0xE50078D7),
            UseAccentForRGB = true,
        };

        public static SolidColorSwitch UColorReversed => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFF000000),
            DarkColorBrush = FromHex(0xFFFFFFFF),
            UseAccentForRGB = true,
        };

        public static SolidColorSwitch UColorReversedReversed => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFFFFFFFF),
            DarkColorBrush = FromHex(0xFF000000),
        };

        /*** Chrome ***/
        public static SolidColorSwitch UColorChromeLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFFF2F2F2),
            DarkColorBrush = FromHex(0xFF171717),
        };

        public static SolidColorSwitch UColorChromeMediumLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFFF2F2F2),
            DarkColorBrush = FromHex(0xFF2B2B2B),
        };

        public static SolidColorSwitch UColorChromeMedium => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFFE6E6E6),
            DarkColorBrush = FromHex(0xFF1F1F1F),
        };

        public static SolidColorSwitch UColorChromeMediumAcrylic => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xCCE6E6E6),
            DarkColorBrush = FromHex(0xCC1F1F1F),
        };

        public static SolidColorSwitch UColorChromeHigh => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFFCCCCCC),
            DarkColorBrush = FromHex(0xFF767676),
        };

        public static SolidColorSwitch UColorChromeHighAcrylic => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xCCCCCCCC),
            DarkColorBrush = FromHex(0xCC767676),
        };

        public static SolidColorSwitch UColorChromeAltLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFF17F7F7),
            DarkColorBrush = FromHex(0xFFF2F2F2),
        };

        public static SolidColorSwitch UColorChromeDisabledLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFF7A7A7A),
            DarkColorBrush = FromHex(0xFF858585),
        };

        public static SolidColorSwitch UColorChromeDisabledHigh => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFFCCCCCC),
            DarkColorBrush = FromHex(0xFF333333),
        };

        public static SolidColorSwitch UColorChromeBlackLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x33000000),
            DarkColorBrush = FromHex(0x33000000),
        };

        public static SolidColorSwitch UColorChromeBlackMediumLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x66000000),
            DarkColorBrush = FromHex(0x66000000),
        };

        public static SolidColorSwitch UColorChromeBlackMedium => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xCC000000),
            DarkColorBrush = FromHex(0xCC000000),
        };

        public static SolidColorSwitch UColorChromeBlackHigh => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFF000000),
            DarkColorBrush = FromHex(0xFF000000),
        };

        public static SolidColorSwitch UColorChromeWhite => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFFFFFFFF),
            DarkColorBrush = FromHex(0xFFFFFFFF),
        };

        /*** ACCENT ***/
        public static SolidColorSwitch UColorHighlightAccent => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFF0078D7),
            DarkColorBrush = FromHex(0xFF0078D7),
            UseAccentForRGB = true,
        };

        public static SolidColorSwitch UColorHighlightAccentLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x330078D7),
            DarkColorBrush = FromHex(0x330078D7),
            UseAccentForRGB = true,
        };

        public static SolidColorSwitch UColorHighlightAccentMediumLow => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x660078D7),
            DarkColorBrush = FromHex(0x660078D7),
            UseAccentForRGB = true,
        };

        public static SolidColorSwitch UColorHighlightAccentMedium => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0x990078D7),
            DarkColorBrush = FromHex(0x990078D7),
            UseAccentForRGB = true,
        };

        public static SolidColorSwitch UColorHighlightAccentMediumHigh => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xCC0078D7),
            DarkColorBrush = FromHex(0xCC0078D7),
            UseAccentForRGB = true,
        };

        public static SolidColorSwitch UColorHighlightAccentHigh => new SolidColorSwitch
        {
            LightColorBrush = FromHex(0xFF0078D7),
            DarkColorBrush = FromHex(0xFF0078D7),
            UseAccentForRGB = true,
        };

    }
}
