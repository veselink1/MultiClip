using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace MultiClip.Controls
{
    /// <summary>
    /// Interaction logic for DragSurface.xaml
    /// </summary>
    public partial class DragSurface : UserControl, IAddChild
    {
        public static readonly DependencyProperty FreezeXAxisProperty = DependencyProperty.Register(
            nameof(FreezeXAxis),
            typeof(bool),
            typeof(DragSurface),
            new PropertyMetadata(false)
            );
        
        public bool FreezeXAxis
        {
            get => (bool)GetValue(FreezeXAxisProperty);
            set => SetValue(FreezeXAxisProperty, value);
        }

        public static readonly DependencyProperty MinXOffsetProperty = DependencyProperty.Register(
            nameof(MinXOffset),
            typeof(double),
            typeof(DragSurface),
            new PropertyMetadata(double.NaN)
            );

        public double MinXOffset
        {
            get => (double)GetValue(MinXOffsetProperty);
            set => SetValue(MinXOffsetProperty, value);
        }

        public static readonly DependencyProperty MaxXOffsetProperty = DependencyProperty.Register(
            nameof(MaxXOffset),
            typeof(double),
            typeof(DragSurface),
            new PropertyMetadata(double.NaN)
            );

        public double MaxXOffset
        {
            get => (double)GetValue(MaxXOffsetProperty);
            set => SetValue(MaxXOffsetProperty, value);
        }

        public static readonly DependencyProperty FreezeYAxisProperty = DependencyProperty.Register(
            nameof(FreezeYAxis),
            typeof(bool),
            typeof(DragSurface),
            new PropertyMetadata(false)
            );

        public bool FreezeYAxis
        {
            get => (bool)GetValue(FreezeYAxisProperty);
            set => SetValue(FreezeYAxisProperty, value);
        }

        public static readonly DependencyProperty MinYOffsetProperty = DependencyProperty.Register(
            nameof(MinYOffset),
            typeof(double),
            typeof(DragSurface),
            new PropertyMetadata(double.NaN)
            );

        public double MinYOffset
        {
            get => (double)GetValue(MinYOffsetProperty);
            set => SetValue(MinYOffsetProperty, value);
        }

        public static readonly DependencyProperty MaxYOffsetProperty = DependencyProperty.Register(
            nameof(MaxYOffset),
            typeof(double),
            typeof(DragSurface),
            new PropertyMetadata(double.NaN)
            );

        public double MaxYOffset
        {
            get => (double)GetValue(MaxYOffsetProperty);
            set => SetValue(MaxYOffsetProperty, value);
        }

        public static readonly RoutedEvent DragStartedEvent = EventManager.RegisterRoutedEvent(
            nameof(DragStarted),
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(DragSurface));

        public event RoutedEventHandler DragStarted
        {
            add { AddHandler(DragStartedEvent, value); }
            remove { RemoveHandler(DragStartedEvent, value); }
        }

        public static readonly RoutedEvent DragMovedEvent = EventManager.RegisterRoutedEvent(
            nameof(DragMoved),
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(DragSurface));

        public event RoutedEventHandler DragMoved
        {
            add { AddHandler(DragMovedEvent, value); }
            remove { RemoveHandler(DragMovedEvent, value); }
        }

        public static readonly RoutedEvent DragEndedEvent = EventManager.RegisterRoutedEvent(
            nameof(DragEnded), 
            RoutingStrategy.Direct, 
            typeof(RoutedEventHandler), 
            typeof(DragSurface));

        public event RoutedEventHandler DragEnded
        {
            add { AddHandler(DragEndedEvent, value); }
            remove { RemoveHandler(DragEndedEvent, value); }
        }

        public Point RelativePosition => _relativePosition;

        private bool _isFirstMouseDown = true;
        private bool _isMoving = false;
        private Point _startPosition = default;
        private Point _relativeMousePosition = default;
        private Point _relativePosition = default;
        private Window _currentWindow = null;

        public DragSurface()
        {
            InitializeComponent();

            Loaded += HandleLoaded;
            PreviewMouseDown += HandleMouseDown;
            PreviewMouseMove += HandleMouseMove;
        }

        private void SubscribeDragCancel()
        {
            _currentWindow = Window.GetWindow(this);
            _currentWindow.PreviewMouseUp += HandleMouseUp;
            _currentWindow.MouseLeave += HandleMouseLeave;
        }

        private void UnsubscribeDragCancel()
        {
            _currentWindow.PreviewMouseUp -= HandleMouseUp;
            _currentWindow.MouseLeave -= HandleMouseLeave;
        }

        private void HandleDragEnded(object sender, RoutedEventArgs e)
        {
            // Unsubscribe from the current window;
            UnsubscribeDragCancel();
            _isMoving = false;
            // Raise the event BEFORE updating the position;
            RoutedEventArgs eventArgs = new RoutedEventArgs(DragEndedEvent, this);
            RaiseEvent(eventArgs);
            if (!eventArgs.Handled)
            {
                ResetPosition();
            }
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            UIElement contentElement = ((UIElement)Content);
            contentElement.RenderTransform = new TranslateTransform();
        }

        private void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isFirstMouseDown)
            {
                _isFirstMouseDown = false;
                GeneralTransform transform = TransformToAncestor(Parent as Visual);
                _startPosition = transform.Transform(new Point(0, 0));
            }

            // Raise the appropriate event;
            RoutedEventArgs eventArgs = new RoutedEventArgs(DragStartedEvent, this);
            RaiseEvent(eventArgs);
            if (eventArgs.Handled)
            {
                _isMoving = false;
            }
            else
            {
                _isMoving = true;
                _relativeMousePosition = Mouse.GetPosition(this);
                // Subscribe for events to the current window;
                SubscribeDragCancel();
            }
        }

        private void HandleMouseLeave(object sender, MouseEventArgs e)
        {
            HandleDragEnded(sender, e);
        }

        private void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            HandleDragEnded(sender, e);
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMoving)
            {
                Point mousePosition = Mouse.GetPosition(Parent as IInputElement);
                Point targetPosition = new Point
                {
                    X = mousePosition.X - _startPosition.X - _relativeMousePosition.X,
                    Y = mousePosition.Y - _startPosition.Y - _relativeMousePosition.Y,
                };

                RoutedEventArgs eventArgs = new RoutedEventArgs(DragMovedEvent, this);
                RaiseEvent(eventArgs);
                if (!eventArgs.Handled)
                {
                    SetPosition(targetPosition);
                }
            }
        }

        public void ResetPosition()
        {
            SetPosition(new Point(0, 0));
        }

        private void SetPosition(Point position)
        {
            UIElement contentElement = ((UIElement)Content);
            TranslateTransform transform = (TranslateTransform)contentElement.RenderTransform;
            if (!FreezeXAxis)
            {
                double xValue = position.X;
                if (!double.IsNaN(MaxXOffset))
                {
                    xValue = Math.Min(xValue, MaxXOffset);
                }
                if (!double.IsNaN(MinXOffset))
                {
                    xValue = Math.Max(xValue, MinXOffset);
                }
                transform.X = xValue;
                _relativePosition.X = xValue;
            }
            if (!FreezeYAxis)
            {
                double yValue = position.X;
                if (!double.IsNaN(MaxYOffset))
                {
                    yValue = Math.Min(yValue, MaxYOffset);
                }
                if (!double.IsNaN(MinYOffset))
                {
                    yValue = Math.Max(yValue, MinYOffset);
                }
                transform.Y = yValue;
                _relativePosition.Y = yValue;
            }
        }

    }
}
