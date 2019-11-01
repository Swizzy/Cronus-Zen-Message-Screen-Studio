using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace CronusZenMessageScreenStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        readonly List<PixelControl> _pixelControls = new List<PixelControl>();
        public MainWindow()
        {
            InitializeComponent();
            for (int x = 0; x < 129; x++)
            {
                TextBlock label = new TextBlock
                                  {
                                      TextAlignment = TextAlignment.Center, Text = x == 0 ? "" : (x - 1).ToString()
                                  };
                Canvas.Children.Add(label);
            }
            for (int y = 0; y < 64; y++)
            {
                TextBlock label = new TextBlock { Text = y.ToString(), TextAlignment = TextAlignment.Right };
                Canvas.Children.Add(label);
                for (int x = 0; x < 128; x++)
                {
                    PixelControl control = new PixelControl(x, y);
                    _pixelControls.Add(control);
                    Canvas.Children.Add(control);
                }
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder data = new StringBuilder();
            data.AppendLine("main {");
            data.AppendLine("\tif (get_val(XB1_A)) {");
            data.AppendLine("\t\tcls_oled(1);");
            foreach (PixelControl pixel in _pixelControls.Where(p => p.Color))
            {
                data.AppendLine($"\t\tpixel_oled({pixel.X}, {pixel.Y}, 1)");
            }
            data.AppendLine("\t}");
            data.AppendLine("}");

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = ".gpc";
            sfd.FileName = "script.gpc";
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, data.ToString());
            }
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
                      {
                          Filter = ImageProcessor.GetFilterString()
                      };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    var img = ImageProcessor.LoadImage(ofd.FileName);
                    img = ImageProcessor.ScaleImage(img, 128, 64);
                    var pixels = ImageProcessor.MakeBinaryMatrix(img);
                    for (int y = 0; y < 64; y++)
                    {
                        for (var x = 0; x < 128; x++)
                        {
                            PixelControl pixel = _pixelControls.First(p => p.X == x && p.Y == y);
                            pixel.Color = pixels[x, y];
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("A error occured while processing the image file you selected:\r\n" + ex.Message, "A Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    width *= 1.1;
                    height *= 1.1;
                }
                else if (e.Delta < 0)
                {
                    width *= 0.9;
                    height *= 0.9;
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
    }
}
