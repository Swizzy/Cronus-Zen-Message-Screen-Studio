using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Image = System.Drawing.Image;

namespace CronusZenMessageScreenStudio
{
    /// <summary>
    ///     Interaction logic for LoadImage.xaml
    /// </summary>
    public partial class LoadImageWindow
    {
        private Bitmap _finalImage;

        private Bitmap _selectedImage;

        public LoadImageWindow()
        {
            InitializeComponent();
            SizeChanged += (sender, args) => System.Diagnostics.Trace.WriteLine($"Width: {ActualWidth} Height: {ActualHeight}");
            LayoutRoot.DataContext = this;
            PositionBox.ItemsSource = ImageProcessor.MakePositionSelectionList();
            InterpolationModeBox.ItemsSource = ImageProcessor.MakeInterpolationSelectionList();
            Threshold = 200;
            MarginTop = 0;
            MarginBottom = 0;
            MarginLeft = 0;
            MarginRight = 0;
            Position = ImageProcessor.Positions.Center;
        }

        public double Threshold { get; set; }
        public int MarginTop { get; set; }
        public int MarginBottom { get; set; }
        public int MarginLeft { get; set; }
        public int MarginRight { get; set; }
        public ImageProcessor.Positions Position { get; set; }
        public bool Invert { get; set; }
        public bool InvertBackground { get; set; }
        public InterpolationMode InterpolationMode { get; set; }


        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
                      {
                          Filter = ImageProcessor.GetFilterString()
                      };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    _selectedImage = ImageProcessor.LoadImage(ofd.FileName);
                    const int width = 512; // * (128 / 64);
                    const int height = 512; // / (128 / 64);
                    if (_selectedImage.Width > width || _selectedImage.Height > height)
                    {
                        _selectedImage = ImageProcessor.ScaleImage(_selectedImage, width, height, InterpolationMode);
                    }
                    UpdatePreview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("A error occured while processing the image file you selected:\r\n" + ex.Message,
                                    "A Error occured",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        private void UpdatePreview()
        {
            if (_selectedImage == null)
            {
                return;
            }

            Bitmap scaledImage = ImageProcessor.ScaleImage(_selectedImage,
                                                           128,
                                                           64,
                                                           ImageProcessor.ScalingTypes.FixedSize,
                                                           Position,
                                                           MarginTop,
                                                           MarginBottom,
                                                           MarginLeft,
                                                           MarginRight,
                                                           InvertBackground ? Color.White : Color.Black,
                                                           InterpolationMode);
            _finalImage = ImageProcessor.MakeBinaryImage(scaledImage, Threshold, Invert);
            ImagePreview.Source = BitmapToImageSource(_finalImage);
        }

        public bool[,] GetPixels() => ImageProcessor.MakeBinaryMatrix(_finalImage, Threshold, false);

        private void PositionBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePreview();

        private void NumericUpDown_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e) => UpdatePreview();



        private void Checkbox_Changed(object sender, RoutedEventArgs e) => UpdatePreview();

        private static BitmapImage BitmapToImageSource(Image image)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                image.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage toReturn = new BitmapImage();
                toReturn.BeginInit();
                toReturn.StreamSource = memory;
                toReturn.CacheOption = BitmapCacheOption.OnLoad;
                toReturn.EndInit();
                return toReturn;
            }
        }

        private void Ok_click(object sender, RoutedEventArgs e)
        {
            if (_finalImage != null)
            {
                DialogResult = true;
            }
            Close();
        }
    }
}
