using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CronusZenMessageScreenStudio
{
    /// <summary>
    /// Interaction logic for PixelControl.xaml
    /// </summary>
    public partial class PixelControl
    {
        private bool _color;
        public int X { get; set; }
        public int Y { get; set; }

        public bool Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    Refresh();
                }
            }
        }

        public PixelControl()
        {
            InitializeComponent();
            Refresh();
            MouseEnter += HandleMouseEvent;
            MouseLeftButtonDown += HandleMouseButtonEvent;
            MouseRightButtonDown += HandleMouseButtonEvent;
        }

        public PixelControl(int x, int y, bool color = false) : this()
        {
            X = x;
            Y = y;
            Color = color;
        }

        private void HandleMouseButtonEvent(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
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

        private void HandleMouseEvent(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Color = true;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                Color = false;
            }
        }

        public void Invert()
        {
            Color = !Color;
        }

        public void Refresh() { Background = Color ? Brushes.White : Brushes.Black; }
    }
}
