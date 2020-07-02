using System;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Drawing.Color;

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

        public Color GetColor()
        {
            if (Color)
            {
                if (Highlighted)
                {
                    return System.Drawing.Color.FromArgb(191, 191, 191);
                }
                return System.Drawing.Color.White;
            }

            if (Highlighted)
            {
                return System.Drawing.Color.FromArgb(64, 64, 64);
            }
            return System.Drawing.Color.Black;
        }

        internal bool IsWithinSquare(int inputX, int inputY, int width, int height)
        {
            width /= 2;
            height /= 2;
            return Y >= inputY - height && Y <= inputY + height && X >= inputX - width && X <= inputX + width;
        }

        public bool IsWithinEllipse(int inputX, int inputY, int width, int height)
        {
            width = Math.Max(width / 2, 1);
            height = Math.Max(height / 2, 1);

            double xLimit = Math.Pow(X - inputX, 2) / Math.Pow(width, 2);
            double yLimit = Math.Pow(Y - inputY, 2) / Math.Pow(height, 2);

            return xLimit + yLimit <= 1.0;
        }

        public bool IsWithinCross(int inputX, int inputY, int width, int height)
        {
            if (inputY == Y)
            {
                width /= 2;
                return X <= inputX + width && X >= inputX - width;
            }
            if (inputX == X)
            {
                height /= 2;
                return Y <= inputY + height && Y >= inputY - height;
            }
            return false;
        }

        private double TriangleArea(int x1, int y1, int x2, int y2, int x3, int y3) => Math.Abs((x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2.0);

        private bool InsideTriangle(int x1, int y1, int x2,
            int y2, int x3, int y3,
            int x, int y)
        {
            /* Calculate area of triangle ABC */
            double a = TriangleArea(x1, y1, x2, y2, x3, y3);

            /* Calculate area of triangle PBC */
            double a1 = TriangleArea(x, y, x2, y2, x3, y3);

            /* Calculate area of triangle PAC */
            double a2 = TriangleArea(x1, y1, x, y, x3, y3);

            /* Calculate area of triangle PAB */
            double a3 = TriangleArea(x1, y1, x2, y2, x, y);

            /* Check if sum of A1, A2 and A3 is same as A */
            return (a == a1 + a2 + a3);
        }

        public bool IsWithinTriangleUp(int inputX, int inputY, int width, int height)
        {
            // Right Corner
            int x1 = inputX + width / 2;
            int y1 = inputY;
            // Left corner
            int x2 = inputX - width / 2;
            int y2 = inputY;
            // Top corner
            int x3 = inputX;
            int y3 = inputY - height;
            return InsideTriangle(x1, y1, x2, y2, x3, y3, X, Y);
        }

        public bool IsWithinTriangleDown(int inputX, int inputY, int width, int height)
        {
            // Right Corner
            int x1 = inputX + width / 2;
            int y1 = inputY;
            // Left corner
            int x2 = inputX - width / 2;
            int y2 = inputY;
            // Bottom corner
            int x3 = inputX;
            int y3 = inputY + height;
            return InsideTriangle(x1, y1, x2, y2, x3, y3, X, Y);
        }

        public bool IsWithinTriangleLeft(int inputX, int inputY, int width, int height)
        {
            // Top corner
            int x1 = inputX;
            int y1 = inputY - height / 2;
            // Left corner
            int x2 = inputX - width;
            int y2 = inputY;
            // Bottom corner
            int x3 = inputX;
            int y3 = inputY + height / 2;
            return InsideTriangle(x1, y1, x2, y2, x3, y3, X, Y);
        }

        public bool IsWithinTriangleRight(int inputX, int inputY, int width, int height)
        {
            // Top corner
            int x1 = inputX;
            int y1 = inputY - height / 2;
            // Left corner
            int x2 = inputX + width;
            int y2 = inputY;
            // Bottom corner
            int x3 = inputX;
            int y3 = inputY + height / 2;
            return InsideTriangle(x1, y1, x2, y2, x3, y3, X, Y);
        }
    }
}
