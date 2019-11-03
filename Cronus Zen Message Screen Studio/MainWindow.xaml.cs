using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CronusZenMessageScreenStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        readonly List<PixelControl> _pixelControls = new List<PixelControl>();
        private PixelControl _lastHighlight;
        private bool _windowLoaded;
        public MainWindow()
        {
            InitializeComponent();
            Title = $"Cronus Zen Message Screen Studio v{App.Version}";

            for (int x = 0; x < 129; x++)
            {
                TextBlock label = new TextBlock
                                  {
                                      TextAlignment = TextAlignment.Center,
                                      Text = x == 0 ? "" : (x - 1).ToString(),
                                      Margin = new Thickness(0,0,5,0)
                                  };
                Canvas.Children.Add(label);
            }
            for (int y = 0; y < 64; y++)
            {
                TextBlock label = new TextBlock
                                  {
                                      Text = y.ToString(),
                                      TextAlignment = TextAlignment.Right,
                                      Margin = new Thickness(5, 0, 5, 0)
                };
                Canvas.Children.Add(label);
                for (int x = 0; x < 128; x++)
                {
                    PixelControl control = new PixelControl(HighlightRowAndColumn, x, y);
                    _pixelControls.Add(control);
                    Canvas.Children.Add(control);
                }
            }
        }

        private void HighlightRowAndColumn(PixelControl control)
        {
            if (control == null)
            {
                return;
            }
            _lastHighlight = control;
            bool setWhite = false;
            bool setBlack = false;

            if (control.Highlighted && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                setWhite = true;
            }
            else if (control.Highlighted && Mouse.RightButton == MouseButtonState.Pressed)
            {
                setBlack = true;
            }
            int x = control.X;
            int y = control.Y;
            CursorPosition.Text = control.Highlighted ? $"X: {x} Y: {y}" : "";
            int thickness = (int)(PenThickness.Value ?? 1) - 1;
            if (HighlightColumnAndRowBox.IsChecked == true || HighlightFullColumnAndRowBox.IsChecked == true || thickness > 0)
            {
                int xMin = x - (thickness / 2);
                int xMax = x + (thickness / 2);
                int yMin = y - (thickness / 2);
                int yMax = y + (thickness / 2);
                foreach (PixelControl pixelControl in _pixelControls)
                {
                    pixelControl.Highlighted = control.Highlighted
                                               &&
                                               (
                                                   HighlightColumnAndRowBox.IsChecked == true
                                                   &&
                                                   (
                                                       pixelControl.X <= x && pixelControl.Y == y
                                                       ||
                                                       pixelControl.Y <= y && pixelControl.X == x
                                                   )
                                                   ||
                                                   HighlightFullColumnAndRowBox.IsChecked == true
                                                   &&
                                                   (
                                                       pixelControl.Y == y
                                                       ||
                                                       pixelControl.X == x
                                                   )
                                                   ||
                                                   (
                                                       (pixelControl.Y >= yMin && pixelControl.Y <= yMax)
                                                       &&
                                                       (pixelControl.X >= xMin && pixelControl.X <= xMax)
                                                    )
                                               );
                    if (setWhite && ((pixelControl.X >= xMin && pixelControl.X <= xMax) && (pixelControl.Y >= yMin && pixelControl.Y <= yMax)))
                    {
                        pixelControl.Color = true;
                    }
                    else if (setBlack && ((pixelControl.X >= xMin && pixelControl.X <= xMax) && (pixelControl.Y >= yMin && pixelControl.Y <= yMax)))
                    {
                        pixelControl.Color = false;
                    }
                }
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e) => new ExportWindow(_pixelControls) { Owner = this }.ShowDialog();

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            var imageWindow = new LoadImageWindow { Owner = this };
            if (imageWindow.ShowDialog() == true)
            {
                var pixels = imageWindow.GetPixels();
                for (int y = 0; y < 64; y++)
                {
                    for (var x = 0; x < 128; x++)
                    {
                        PixelControl pixel = _pixelControls.First(p => p.X == x && p.Y == y);
                        pixel.Color = pixels[x, y];
                    }
                }
            }
        }

        private void ViewBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Handled = true;
                var width = Viewbox.ActualWidth;
                var height = Viewbox.ActualHeight;
                if (e.Delta > 0)
                {
                    width *= 1.02;
                    height *= 1.02;
                }
                else if (e.Delta < 0)
                {
                    width *= 0.98;
                    height *= 0.98;
                }

                Viewbox.Width = width;
                Viewbox.Height = height;
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear all pixels?", "", MessageBoxButton.YesNoCancel) ==
                MessageBoxResult.Yes)
            {
                foreach (PixelControl pixel in _pixelControls)
                {
                    pixel.Color = false;
                }
            }
        }

        private void Invert_Click(object sender, RoutedEventArgs e)
        {
            foreach (PixelControl pixel in _pixelControls)
            {
                pixel.Invert();
            }
        }

        private void MainWindow_OnActivated(object sender, EventArgs eventArgs)
        {
            if (_windowLoaded)
            {
                return;
            }
            ShowAllPixels_Click(this, null);
            _windowLoaded = true;
        }

        private void PenThickness_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e) => HighlightRowAndColumn(_lastHighlight);

        private void ShowAllPixels_Click(object sender, RoutedEventArgs e)
        {
            Viewbox.Width = Viewbox.MinWidth = ScrollViewer.ViewportWidth;
            Viewbox.Height = Viewbox.MinHeight = ScrollViewer.ViewportHeight;
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var bw = new BackgroundWorker();
            bw.DoWork += (o, args) => Thread.Sleep(100);
            bw.RunWorkerCompleted += (o, args) =>
                                     {
                                         Viewbox.MinWidth = ScrollViewer.ViewportWidth;
                                         Viewbox.MinHeight = ScrollViewer.ViewportHeight;
                                     };
            bw.RunWorkerAsync();
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e) { MainWindow_OnSizeChanged(sender, null); }
    }
}
