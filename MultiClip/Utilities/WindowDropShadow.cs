using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace MultiClip.Utilities
{
    public class WindowDropShadow : DependencyObject
    {
        private Window _window;
        private Window _target;

        /// <summary>
        /// <see cref="Radius"/>
        /// </summary>
        public static DependencyProperty RadiusProperty = DependencyProperty.Register(
            nameof(Radius),
            typeof(double),
            typeof(WindowDropShadow),
            new PropertyMetadata(0.0));

        /// <summary>
        /// The radius of the shadow.
        /// </summary>
        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        /// <summary>
        /// <see cref="Strength"/>
        /// </summary>
        public static DependencyProperty StrengthProperty = DependencyProperty.Register(
            nameof(Strength),
            typeof(double),
            typeof(WindowDropShadow),
            new PropertyMetadata(0.0));

        /// <summary>
        /// The radius of the shadow.
        /// </summary>
        public double Strength
        {
            get => (double)GetValue(StrengthProperty);
            set => SetValue(StrengthProperty, value);
        }

        /// <summary>
        /// <see cref="Opacity"/>
        /// </summary>
        public static DependencyProperty OpacityProperty = UIElement.OpacityProperty;

        /// <summary>
        /// The radius of the shadow.
        /// </summary>
        public double Opacity
        {
            get => (_window.Content as Border).Opacity;
            set => (_window.Content as Border).Opacity = value;
        }

        public WindowDropShadow(Window target)
        {
            _target = target;
            _window = new Window
            {
                Topmost = true,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                Content = new Border
                {
                    BorderBrush = Brushes.Black,
                },
            };
            _window.SourceInitialized += OnSourceInitialized;
            target.LocationChanged += OnTargetLocationChanged;
            target.Closed += OnTargetClosed;
        }

        public void Show()
        {
            _window.Show();
        }

        public void Hide()
        {
            _window.Hide();
        }

        public void BeginAnimation(DependencyProperty dp, AnimationTimeline animation)
        {
            (_window.Content as Border).BeginAnimation(dp, animation);
        }

        private void OnTargetClosed(object sender, EventArgs e)
        {
            _window.Close();
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            WindowStyles.SetExtended(_window, WindowStyles.GetExtended(_window)
                | ExtendedWindowStyle.WS_EX_NOACTIVATE
                | ExtendedWindowStyle.WS_EX_TOOLWINDOW
                | ExtendedWindowStyle.WS_EX_TOPMOST);
            OnTargetLocationChanged(_window, new EventArgs());
        }

        private void UpdateProperties()
        {
            var border = (Border)_window.Content;
            border.BorderThickness = new Thickness(Strength);
            border.BorderBrush = new SolidColorBrush(new Color { A = 255, R = 0, G = 0, B = 0 });
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            var border = (_window.Content as Border);
            switch (e.Property.Name)
            {
                case nameof(Radius):
                    border.Margin = new Thickness(Radius - Strength / 2);
                    border.Effect = new BlurEffect
                    {
                        Radius = Radius,
                        KernelType = KernelType.Gaussian,
                        RenderingBias = RenderingBias.Performance
                    };
                    break;
                case nameof(Opacity):
                    _window.Opacity = Opacity;
                    break;
                case nameof(Strength):
                    border.BorderThickness = new Thickness(Strength);
                    border.Margin = new Thickness(Radius - Strength / 2);
                    break;
            }
        }

        private void OnTargetLocationChanged(object sender, EventArgs e)
        {
            _window.Width = _target.Width + Radius * 2;
            _window.Height = _target.Height + Radius * 2;
            _window.Top = _target.Top - Radius;
            _window.Left = _target.Left - Radius;
        }
    }
}
