using System.Windows.Media;

namespace UComponents.Themes
{
    public class SolidColorSwitch
    {
        public SolidColorBrush LightColorBrush { get; set; }
        public SolidColorBrush DarkColorBrush { get; set; }
        public bool UseAccentForRGB { get; set; } = false;
    }
}
