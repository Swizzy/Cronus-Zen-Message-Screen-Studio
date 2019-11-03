using System;
using System.Windows.Input;
using System.Windows.Media;

namespace CronusZenMessageScreenStudio
{
    /// <summary>
    /// Interaction logic for PixelControl.xaml
    /// </summary>
    public partial class PixelControl
    {
        private readonly Action<PixelControl> _highlightRowAndColumn;
        private bool _color, _colorChanged;
        private bool _highlighted, _highlightedChanged;
        public int X { get; set; }
        public int Y { get; set; }

        public bool Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _colorChanged = true;
                    _color = value;
                    Refresh();
                    _colorChanged = false;
                }
            }
        }

        public bool Highlighted
        {
            get => _highlighted;
            set
            {
                if (_highlighted != value)
                {
                    _highlightedChanged = true;
                    _highlighted = value;
                    Refresh();
                    _highlightedChanged = false;
                }
            }
        }

        public PixelControl()
        {
            InitializeComponent();
            Refresh();
            MouseEnter += HandleMouseEnter;
            MouseLeave += HandleMouseLeave;
            MouseLeftButtonDown += HandleMouseButtonEvent;
            MouseRightButtonDown += HandleMouseButtonEvent;
        }

        public PixelControl(Action<PixelControl> highlightRowAndColumn, int x, int y, bool color = false) : this()
        {
            _highlightRowAndColumn = highlightRowAndColumn;
            X = x;
            Y = y;
            Color = color;
        }

        private void HandleMouseButtonEvent(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                _highlightRowAndColumn(this);
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        Color = true;
                        break;
                    case MouseButton.Right:
                        Color = false;
                        break;
                }
            }
        }

        private void HandleMouseEnter(object sender, MouseEventArgs e)
        {
            Highlighted = true;
            _highlightRowAndColumn(this);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Color = true;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                Color = false;
            }
        }

        private void HandleMouseLeave(object sender, MouseEventArgs e)
        {
            Highlighted = false;
            _highlightRowAndColumn(this);
        }

        public void Invert()
        {
            Color = !Color;
        }

        public void Refresh()
        {
            if (_colorChanged)
            {
                Background = Color ? Brushes.White : Brushes.Black;
            }

            if (_highlightedChanged || Highlighted && _colorChanged)
            {
                if (Highlighted)
                {
                    Overlay.Background = Color ? Brushes.Black : Brushes.White;
                }
                else
                {
                    Overlay.Background = Brushes.Transparent;
                }
            }
        }
    }
}
