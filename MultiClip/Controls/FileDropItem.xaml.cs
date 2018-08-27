using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Shell;
using MultiClip.Native;
using MultiClip.Utilities;

namespace MultiClip.Controls
{
    /// <summary>
    /// Interaction logic for FileDropItem.xaml
    /// </summary>
    public partial class FileDropItem : UserControl
    {
        public static event Action PreviewShellExecute = delegate { };

        public static readonly DependencyProperty SourcePathProperty = DependencyProperty.Register(
            nameof(SourcePath),
            typeof(string),
            typeof(FileDropItem),
            new PropertyMetadata(null)
        );

        public string SourcePath
        {
            get => (string)GetValue(SourcePathProperty);
            set => SetValue(SourcePathProperty, value);
        }

        private static readonly Lazy<BitmapSource> DefaultThumbnail = new Lazy<BitmapSource>(() =>
        {
            string defaultThumbnailPath = System.IO.Path.Combine(Environment.SystemDirectory, "user32.dll");
            ShellThumbnail defaultThumbnail = ShellObject.FromParsingName(defaultThumbnailPath).Thumbnail;
            BitmapSource bitmapSource = defaultThumbnail.SmallBitmapSource;
            bitmapSource.Freeze();
            return bitmapSource;
        });

        public FileDropItem()
        {
            InitializeComponent();
            ViewButton.Click += ViewButton_Click;
            ThumbnailImage.IsVisibleChanged += ThumbnailImage_IsVisibleChanged;
        }

        private void ThumbnailImage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Image image = (Image)sender;
            if (image.IsVisible
                && image.Source == null
                && SourcePath != null)
            {
                try
                {
                    ShellThumbnail thumbnail = ShellObject.FromParsingName(SourcePath).Thumbnail;
                    BitmapSource source = thumbnail.SmallBitmapSource;
                    image.Source = source;
                }
                catch (Exception ex)
                {
                    Logger.Default.LogWarn(LogEvents.UiOpErr, ex);
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == nameof(SourcePath))
            {
                ContentText.Text = SourcePath.Length > 24
                    ? SourcePath.Substring(0, 3) + "..." + SourcePath.Substring(SourcePath.Length - 18)
                    : SourcePath;
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            PreviewShellExecute.Invoke();

            var sxi = new NativeMethods.SHELLEXECUTEINFO();
            sxi.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(sxi);
            sxi.lpVerb = "Properties";
            sxi.nShow = NativeMethods.SW_SHOW;
            sxi.fMask = NativeMethods.SEE_MASK_INVOKEIDLIST;
            if (File.Exists(SourcePath))
            {
                sxi.lpFile = SourcePath;
            }
            else if (Directory.Exists(SourcePath))
            {
                sxi.lpDirectory = SourcePath;
            }
            else
            {
                MessageBox.Show("The file or folder you are trying to open could not be found. This may happed if the target of the operation has been moved or deleted.", "Oops! An error occurred...", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            if (!NativeMethods.ShellExecuteEx(ref sxi))
            {
                throw new System.ComponentModel.Win32Exception();
            }
        }
    }
}
