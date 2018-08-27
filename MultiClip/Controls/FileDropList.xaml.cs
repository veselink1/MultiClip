using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MultiClip.Utilities;

namespace MultiClip.Controls
{
    /// <summary>
    /// Interaction logic for FileDropList.xaml
    /// </summary>
    public partial class FileDropList : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IList<string>),
            typeof(FileDropList),
            new PropertyMetadata(null)
        );

        public IList<string> ItemsSource
        {
            get => (IList<string>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private const int MaxItems = 20;

        public FileDropList()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateContent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name == nameof(ItemsSource) && e.OldValue != e.NewValue)
            {
                UpdateContent();
            }
        }

        private void UpdateContent()
        {
            ListView itemList = (ListView)this.FindUid("ListBox");
            if (itemList != null)
            {
                if (itemList.ItemsSource != ItemsSource)
                {
                    itemList.ItemsSource = ItemsSource.Take(MaxItems).ToList();
                }
            }

            TextBlock totalItemsText = (TextBlock)this.FindUid("TotalItemsText");
            if (totalItemsText != null)
            {
                totalItemsText.Text = "Total items: " + ItemsSource.Count;
            }

            TextBlock remainingItemsText = (TextBlock)this.FindUid("RemainingItemsText");
            if (remainingItemsText != null)
            {
                if (ItemsSource.Count > MaxItems)
                {
                    remainingItemsText.Text = (ItemsSource.Count - MaxItems) + " more items...";
                    remainingItemsText.Visibility = Visibility.Visible;
                }
                else
                {
                    remainingItemsText.Text = null;
                    remainingItemsText.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
