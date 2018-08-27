using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MultiClip.ViewModels;

namespace MultiClip.Controls
{
    /// <summary>
    /// Interaction logic for ClipboardList.xaml
    /// </summary>
    public partial class ClipboardList : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable<ClipboardViewModel>),
            typeof(ClipboardList),
            new PropertyMetadata(null)
        );

        public IEnumerable<ClipboardViewModel> ItemsSource
        {
            get => (IEnumerable<ClipboardViewModel>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ClipboardList()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
