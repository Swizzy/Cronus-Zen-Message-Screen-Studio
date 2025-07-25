using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
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
    public partial class LoadImageWindow : INotifyPropertyChanged
    {
        private bool[,] _finalImage;
        private bool[,] _currentImage;
        private Bitmap _selectedImage, _originalSelectedImage;
        private double _rgbThreshold, _hslThreshold;
        private int _selectedFrame;
        public event PropertyChangedEventHandler PropertyChanged;

        public LoadImageWindow(bool[,] currentImage)
        {
            _currentImage = currentImage;
            InitializeComponent();
            SizeChanged += (sender, args) => System.Diagnostics.Trace.WriteLine($"Width: {ActualWidth} Height: {ActualHeight}");
            LayoutRoot.DataContext = this;
            PositionBox.ItemsSource = ImageProcessor.MakePositionSelectionList();
            InterpolationModeBox.ItemsSource = ImageProcessor.MakeInterpolationSelectionList();
            DitheringAlgorithmBox.ItemsSource = ImageProcessor.MakeDitheringSelectionList();
            RGBThreshold = 200;
            HSLThreshold = 50;
            UseHSL = true;
            MarginTop = 0;
            MarginBottom = 0;
            MarginLeft = 0;
            MarginRight = 0;
            Position = ImageProcessor.Positions.Center;
            UpdatePreview();
        }

        public double RGBThreshold
        {
            get => _rgbThreshold;
            set
            {
                _rgbThreshold = value;
                OnPropertyChanged();
            }
        }

        public double HSLThreshold
        {
            get => _hslThreshold;
            set
            {
                _hslThreshold = value;
                OnPropertyChanged();
            }
        }

        public bool UseHSL { get; set; }
        public int MarginTop { get; set; }
        public int MarginBottom { get; set; }
        public int MarginLeft { get; set; }
        public int MarginRight { get; set; }
        public ImageProcessor.Positions Position { get; set; }
        public bool Invert { get; set; }
        public bool InvertBackground { get; set; }
        public InterpolationMode InterpolationMode { get; set; }
        public ImageProcessor.DitheringAlgorithms DitheringAlgorithm { get; set; }

        public int SelectedFrame
        {
            get => _selectedFrame;
            set
            {
                _selectedFrame = value;
                OnPropertyChanged();
            }
        }

        public int MaxFrames { get; set; }

        public bool MergeWhites { get; set; }
        public bool MergeBlacks { get; set; }

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
                    _originalSelectedImage = _selectedImage = ImageProcessor.LoadImage(ofd.FileName);
                    SelectedFrame = 0;
                    MaxFrames = ImageProcessor.GetFrameCount(_selectedImage);
                    OnPropertyChanged(nameof(MaxFrames));
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
            if (UseHSL)
            {
                SetThresholdVisibility(Visibility.Collapsed, Visibility.Visible);
            }
            else
            {
                SetThresholdVisibility(Visibility.Visible, Visibility.Collapsed);
            }

            if (_selectedImage == null)
            {
                return;
            }

            _selectedImage = ImageProcessor.SetFrame(_originalSelectedImage, SelectedFrame);

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
            _finalImage = ImageProcessor.MakeBinaryMatrix(scaledImage, UseHSL ? (HSLThreshold / 100) : RGBThreshold, Invert, UseHSL);
            if (MergeWhites)
            {
                _finalImage = ImageProcessor.MergeWhites(_currentImage, _finalImage);
            }
            else if (MergeBlacks)
            {
                _finalImage = ImageProcessor.MergeBlacks(_currentImage, _finalImage);
            }
            ImagePreview.Source = BitmapToImageSource(ImageProcessor.MakeBinaryImage(_finalImage, scaledImage.Width, scaledImage.Height));
        }

        public bool[,] GetPixels() => _finalImage;

        private void PositionBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePreview();

        private void NumericUpDown_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e) => UpdatePreview();

        private void Checkbox_Changed(object sender, RoutedEventArgs e) => UpdatePreview();

        private void SetThresholdVisibility(Visibility rgb, Visibility hsl)
        {
            if (RGBslider != null)
            {
                RGBslider.Visibility = rgb;
            }
            if (RGBnupd != null)
            {
                RGBnupd.Visibility = rgb;
            }
            if (HSLslider != null)
            {
                HSLslider.Visibility = hsl;
            }
            if (HSLnupd != null)
            {
                HSLnupd.Visibility = hsl;
            }
        }

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

        private void Slider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdatePreview();

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
