using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UComponents
{
    public class Button : System.Windows.Controls.Button
    {
        static Button()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Button), new FrameworkPropertyMetadata(typeof(Button)));
        }

        public static readonly DependencyProperty IsPointerOverProperty = DependencyProperty.Register(
            nameof(IsPointerOver),
            typeof(bool),
            typeof(Button),
            new PropertyMetadata(false)
        );

        public bool IsPointerOver
        {
            get => (bool)GetValue(IsPointerOverProperty);
            private set => SetValue(IsPointerOverProperty, value);
        }

        private bool _isMouseDown = false;
        private InteractiveSurface _interactiveSurface;

        public Button()
        {
            Loaded += Button_Loaded;
            
            MouseEnter += Button_MouseEnter;
            MouseLeave += Button_MouseLeave;

            MouseLeftButtonDown += Button_MouseDown;
            MouseLeftButtonUp += Button_MouseUp;
        }

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            _interactiveSurface = Template.FindName("InteractiveSurface", this) as InteractiveSurface;
        }

        private void Button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            switch (ClickMode)
            {
                case ClickMode.Release:
                    CaptureMouse();
                    _isMouseDown = true;
                    IsPressed = true;
                    break;
                case ClickMode.Hover:
                    _isMouseDown = true;
                    IsPressed = true;
                    break;
                case ClickMode.Press:
                    _isMouseDown = true;
                    IsPressed = true;
                    break;
            }
        }

        private void Button_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            switch (ClickMode)
            {
                case ClickMode.Release:
                    if (IsMouseCaptured)
                    {
                        ReleaseMouseCapture();
                    }

                    // Need to manually release the surface,
                    // since ReleaseMouseCapture() blocks mouse up events!
                    _interactiveSurface.IsActive = false;

                    if (_isMouseDown && IsPointerOver)
                    {
                        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
                    }

                    _isMouseDown = false;
                    IsPressed = false;
                    break;
                case ClickMode.Hover:
                    _isMouseDown = false;
                    break;
                case ClickMode.Press:
                    if (_isMouseDown)
                    {
                        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
                    }
                    _isMouseDown = false;
                    break;
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            IsPointerOver = true;
            switch (ClickMode)
            {
                case ClickMode.Release:
                    if (_isMouseDown) IsPressed = true;
                    break;
                case ClickMode.Hover:
                    IsPressed = true;
                    RaiseEvent(new RoutedEventArgs(ClickEvent, this));
                    break;
                case ClickMode.Press:
                    break;
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            IsPressed = false;
            IsPointerOver = false;
        }
    }
}
