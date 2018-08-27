using System;
using System.Windows;
using MultiClip.Intl;
using MultiClip.Utilities;
using UComponents.Themes;

namespace MultiClip.Windows
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            WindowIconHelper.HideIcon(this);
            IntlManager.Reapply();
            ThemeManager.Reapply();
        }
    }
}
