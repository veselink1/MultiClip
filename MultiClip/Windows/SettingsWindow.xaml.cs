using System;
using System.Windows;
using System.Windows.Controls;
using MultiClip.Intl;
using MultiClip.Models;
using MultiClip.Utilities;
using MultiClip.ViewModels;
using UComponents.Themes;

namespace MultiClip.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = _viewModel = new SettingsViewModel(this, AppState.Current.UserSettings);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowIconHelper.HideIcon(this);
            IntlManager.Reapply();
            ThemeManager.Reapply();
            ContentFrame.Child = (UIElement)ContentFrame.Resources["General"];
        }

        private void MenuItem_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            string target = (string)element.Resources["Target"];

            UIElement content = (UIElement)ContentFrame.Resources[target];
            ContentFrame.Child = content;
        }

        private FrameworkElement GetRootElement(FrameworkElement element)
        {
            while (element.Parent is FrameworkElement)
            {
                element = (FrameworkElement)element.Parent;
            }
            return element;
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            new HelpWindow { Owner = this }.Show();
        }
    }
}
