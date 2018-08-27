using System.Windows;

namespace UComponents
{
    public class CheckBox : ToggleButton
    {
        static CheckBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckBox), new FrameworkPropertyMetadata(typeof(CheckBox)));
        }

        public CheckBox()
        {
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        private void OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsMouseDown = true;
        }

        private void OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsMouseDown)
            {
                IsChecked = !IsChecked;
            }
            IsMouseDown = false;
        }
    }
}
