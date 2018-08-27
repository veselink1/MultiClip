using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MultiClip.Controls;
using MultiClip.Utilities;
using UComponents.Themes;

namespace MultiClip.Windows
{
    /// <summary>
    /// Interaction logic for QuickPasteWindow.xaml
    /// </summary>
    public partial class QuickInfoWindow : Window
    {
        public event MouseButtonEventHandler Click;
        
        private bool _isVisible = false;
        private bool _isShowingInfo = false;

        private const double DesignWidth = 380;
        private const double DesignHeight = 220;

        private const double DesignRightMargin = 16;
        private const double DesignBottomMargin = 12;

        public QuickInfoWindow()
        {
            InitializeComponent();

            Width = DesignWidth + DesignRightMargin;
            Height = DesignHeight + DesignBottomMargin;

            Top = SystemParameters.WorkArea.Bottom - Height;
            Left = SystemParameters.WorkArea.Right - Width;

            Container.Margin = new Thickness(left: 0, top: 0, right: DesignRightMargin, bottom: DesignBottomMargin);
            ContentTransform.X = Width;
            ContentTransform.Y = 0;

            Loaded += HandleLoaded;
            MouseDown += HandleMouseDown;
            MouseUp += HandleMouseUp;
            MouseLeave += HandleMouseLeave;
            MouseMove += HandleMouseMove;
            Click += HandleWindowClicked;
            Show();
        }

        private async void HandleWindowClicked(object sender, MouseButtonEventArgs e)
        {
            _isVisible = true;
            await FlyOutAsync();
        }

        #region Mouse Click Handling
        private bool _isMouseDownClick = false;

        private void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDownClick = true;
        }

        private void HandleMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isMouseDownClick = false;
        }

        private void HandleMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isMouseDownClick = false;
        }

        private void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMouseDownClick)
            {
                _isMouseDownClick = false;
                Click?.Invoke(sender, e);
            }
        }
        #endregion

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            WindowStyles.SetExtended(this, WindowStyles.GetExtended(this) 
                | ExtendedWindowStyle.WS_EX_TOOLWINDOW
                | ExtendedWindowStyle.WS_EX_NOACTIVATE
                | ExtendedWindowStyle.WS_EX_TOPMOST);
            ThemeManager.Reapply();
            Hide();
        }

        public async Task ShowContentAsync(UIElement element, TimeSpan timeSpan = default)
        {
            ContentBorder.Child = element;
            if (_isShowingInfo)
            {
                return;
            }
            _isShowingInfo = true;
            Show();
            await FlyInAsync();
            await Task.Delay((int)timeSpan.TotalMilliseconds);
            await FlyOutAsync();
            Hide();
            _isShowingInfo = false;
            ContentBorder.Child = null;
        }

        public Task FlyInAsync()
        {
            if (!_isVisible)
            {
                _isVisible = true;
                var animation = new DoubleAnimation
                {
                    From = ContentTransform.X,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(400),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                };

                ContentTransform.BeginAnimation(TranslateTransform.XProperty, animation);
                return Task.Delay(animation.Duration.TimeSpan);
            }
            return Task.FromResult<object>(null);
        }

        public Task FlyOutAsync()
        {
            if (_isVisible)
            {
                _isVisible = false;
                var animation = new DoubleAnimation
                {
                    From = ContentTransform.X,
                    To = Width,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                };

                ContentTransform.BeginAnimation(TranslateTransform.XProperty, animation);
                return Task.Delay(animation.Duration.TimeSpan);
            }
            return Task.FromResult<object>(null);
        }

        private async void HandlePaneDragEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                DragSurface panel = (DragSurface)sender;
                if (panel.RelativePosition.X > 80)
                {
                    e.Handled = true;
                    (panel.Content as Border).Child = null;
                    _isVisible = true;
                    await FlyOutAsync();
                    panel.ResetPosition();
                }
            }
            catch (Exception ex)
            {
                Logger.Default.LogWarn(LogEvents.UiOpErr, ex);
            }
        }
    }
}
