using System.Linq;
using System.Windows;

namespace UComponents
{
    public class RadioButton : ToggleButton
    {
        static RadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioButton), new FrameworkPropertyMetadata(typeof(RadioButton)));
        }

        public RadioButton()
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
                IsChecked = true;
            }
            IsMouseDown = false;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsCheckedProperty && IsChecked == true)
            {
                foreach (var other in EnumerateGroup().Where(x => x != this))
                {
                    other.IsChecked = false;
                }
            }
        }
    }
}
