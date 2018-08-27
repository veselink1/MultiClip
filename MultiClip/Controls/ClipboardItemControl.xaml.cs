using System.Windows;
using System.Windows.Controls;
using MultiClip.ViewModels;

namespace MultiClip.Controls
{
    /// <summary>
    /// Interaction logic for ClipboardItemControl.xaml
    /// </summary>
    public partial class ClipboardItemControl : UserControl
    {
        public static readonly DependencyProperty ContentSourceProperty = DependencyProperty.Register(
            nameof(ContentSource),
            typeof(ClipboardViewModel),
            typeof(ClipboardItemControl),
            new PropertyMetadata(null));

        public ClipboardViewModel ContentSource
        {
            get => (ClipboardViewModel)GetValue(ContentSourceProperty);
            set => SetValue(ContentSourceProperty, value);
        }

        public ClipboardItemControl()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ContentSourceProperty)
            {
                DataContext = ContentSource;
                Content = ContentSource != null
                    ? Resources[ContentSource.GetType()]
                    : null;
            }
        }
    }
}
