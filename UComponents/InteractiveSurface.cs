using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UComponents
{
    class InteractiveSurface : ContentControl
    {
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(InteractiveSurface),
            new PropertyMetadata(false)
        );

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        static InteractiveSurface()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InteractiveSurface), new FrameworkPropertyMetadata(typeof(InteractiveSurface)));
        }

        public InteractiveSurface()
        {
            PreviewMouseLeftButtonDown += InteractiveSurface_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += InteractiveSurface_PreviewMouseLeftButtonUp;
        }

        private void InteractiveSurface_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsActive = true;
        }

        private void InteractiveSurface_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsActive = false;
        }
    }
}
