using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UComponents
{
    public abstract class ToggleButton : ContentControl
    {
        public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent(
            nameof(Checked),
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(ToggleButton)
        );

        public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent(
            nameof(Unchecked),
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(ToggleButton)
        );

        public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(
            nameof(Group),
            typeof(string),
            typeof(ToggleButton),
            new PropertyMetadata(null)
        );

        public string Group
        {
            get => (string)GetValue(GroupProperty);
            set => SetValue(GroupProperty, value);
        }

        public event RoutedEventHandler Checked
        {
            add => AddHandler(CheckedEvent, value);
            remove => RemoveHandler(CheckedEvent, value);
        }
        
        public event RoutedEventHandler Unchecked
        {
            add => AddHandler(UncheckedEvent, value);
            remove => RemoveHandler(UncheckedEvent, value);
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool),
            typeof(ToggleButton),
            new PropertyMetadata(false)
        );

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty IsMouseDownProperty = DependencyProperty.Register(
            nameof(IsMouseDown),
            typeof(bool),
            typeof(ToggleButton),
            new PropertyMetadata(false)
        );

        public bool IsMouseDown
        {
            get => (bool)GetValue(IsMouseDownProperty);
            protected set => SetValue(IsMouseDownProperty, value);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsCheckedProperty)
            {
                if ((bool)e.NewValue)
                {
                    RaiseEvent(new RoutedEventArgs(CheckedEvent, this));
                }
                else
                {
                    RaiseEvent(new RoutedEventArgs(UncheckedEvent, this));
                }
            }
        }

        protected IEnumerable<ToggleButton> EnumerateGroup()
        {
            UIElement parent = GetTopParent();
            foreach (var element in EnumerateVisualChildrenRecursive(parent))
            {
                if (element is ToggleButton toggleButton)
                {
                    if (toggleButton.Group == Group)
                    {
                        yield return toggleButton;
                    }
                }
            }
        }

        private IEnumerable<UIElement> EnumerateVisualChildrenRecursive(UIElement element)
        {
            if (element != null)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(element);
                for (int i = 0; i < childrenCount; i++)
                {
                    UIElement child = VisualTreeHelper.GetChild(element, i) as UIElement;
                    yield return child;
                    foreach (var subchild in EnumerateVisualChildrenRecursive(child))
                    {
                        yield return subchild;
                    }
                }
            }
        }

        private UIElement GetTopParent()
        {
            UIElement parent = this;
            while (parent != null)
            {
                UIElement nextParent = VisualTreeHelper.GetParent(parent) as UIElement;
                if (nextParent == null)
                {
                    return parent;
                }
                parent = nextParent;
            }
            return parent;
        }
    }
}
