using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiClip.Controls
{
    /// <summary>
    /// Interaction logic for AutoScrollViewer.xaml
    /// </summary>
    public partial class AutoScrollViewer : UserControl
    {
        private enum ScrollState
        {
            Idle,
            ScrollingUp,
            ScrollingDown,
        }

        private ScrollState _scrollState = ScrollState.Idle;
        private readonly float _scrollSpeed = 400f; // pixels per seconds

        private Stopwatch _stopwatch = new Stopwatch();
        private Border _scrollUpBorder = null;
        private Border _scrollDownBorder = null;
        private ScrollViewer _scrollViewer = null;

        public AutoScrollViewer()
        {
            InitializeComponent();
            CompositionTarget.Rendering += OnRendering;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            double deltaTime = _stopwatch.ElapsedMilliseconds / 1000.0;
            if (_scrollState != ScrollState.Idle)
            {
                double change = _scrollState == ScrollState.ScrollingUp
                    ? _scrollSpeed * deltaTime * -1
                    : _scrollSpeed * deltaTime;
                
                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset + change);
            }
            _stopwatch.Restart();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _scrollUpBorder = (Border)Template.FindName("ScrollUpBorder", this);
            _scrollDownBorder = (Border)Template.FindName("ScrollDownBorder", this);
            _scrollViewer = (ScrollViewer)Template.FindName("ScrollViewer", this);

            _scrollUpBorder.MouseEnter += ScrollUpBorder_MouseEnter;
            _scrollUpBorder.MouseLeave += ScrollUpBorder_MouseLeave;
            _scrollDownBorder.MouseEnter += ScrollDownBorder_MouseEnter;
            _scrollDownBorder.MouseLeave += ScrollDownBorder_MouseLeave;
        }

        private void ScrollUpBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            _scrollState = ScrollState.ScrollingUp;
        }

        private void ScrollUpBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            _scrollState = ScrollState.Idle;
        }

        private void ScrollDownBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            _scrollState = ScrollState.ScrollingDown;
        }

        private void ScrollDownBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            _scrollState = ScrollState.Idle;
        }
    }
}
