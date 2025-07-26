using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FontStyle = System.Drawing.FontStyle;
using Image = System.Drawing.Image;

namespace CronusZenMessageScreenStudio
{
    /// <summary>
    ///     Interaction logic for DrawTextWindow.xaml
    /// </summary>
    public partial class DrawTextWindow : INotifyPropertyChanged
    {
        private bool[,] _finalImage;
        private bool[,] _currentImage;
        private double _rgbThreshold, _hslThreshold;
        public event PropertyChangedEventHandler PropertyChanged;

        public DrawTextWindow(bool[,] currentImage)
        {
            _currentImage = currentImage;
            InitializeComponent();
            SizeChanged += (sender, args) => System.Diagnostics.Trace.WriteLine($"Width: {ActualWidth} Height: {ActualHeight}");
            LayoutRoot.DataContext = this;
            TextFontSize = 20;
            PositionBox.ItemsSource = ImageProcessor.MakePositionSelectionList();
            InterpolationModeBox.ItemsSource = ImageProcessor.MakeInterpolationSelectionList();
            using (var installedFontCollection = new InstalledFontCollection())
            {
                foreach (FontFamily fontFamily in installedFontCollection.Families)
                {
                    FontBox.Items.Add(fontFamily);
                }
            }
            TextFontFamily = (FontFamily)FontBox.Items[0];
            RGBThreshold = 200;
            HSLThreshold = 50;
            UseHSL = true;
            MarginTop = 0;
            MarginBottom = 0;
            MarginLeft = 0;
            MarginRight = 0;
            Position = ImageProcessor.Positions.Center;
            WhiteOnBlack = true;
            RefreshPreview();
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
        public InterpolationMode InterpolationMode { get; set; }
        public FontFamily TextFontFamily { get; set; }
        public int TextFontSize { get; set; }
        public bool BoldFont { get; set; }
        public bool ItalicFont { get; set; }
        public bool UnderlineFont { get; set; }
        public bool StrikeoutFont { get; set; }

        public bool WhiteOnBlack { get; set; }

        public bool MergeWhites { get; set; }
        public bool MergeBlacks { get; set; }

        private void RefreshPreview()
        {
            if (UseHSL)
            {
                SetThresholdVisibility(Visibility.Collapsed, Visibility.Visible);
            }
            else
            {
                SetThresholdVisibility(Visibility.Visible, Visibility.Collapsed);
            }
            try
            {
                FontStyle fontStyle = System.Drawing.FontStyle.Regular;
                if (BoldFont)
                    fontStyle |= System.Drawing.FontStyle.Bold;
                if (ItalicFont)
                    fontStyle |= System.Drawing.FontStyle.Italic;
                if (UnderlineFont)
                    fontStyle |= System.Drawing.FontStyle.Underline;
                if (StrikeoutFont)
                    fontStyle |= System.Drawing.FontStyle.Strikeout;

                FontFamily fontFamily = TextFontFamily;
                Font font = new Font(fontFamily, TextFontSize, fontStyle);
                Bitmap img = ImageProcessor.DrawText(string.IsNullOrWhiteSpace(TextBox.Text) ? " " : TextBox.Text, font, WhiteOnBlack);
                Color backgroundColor = WhiteOnBlack ? Color.Black : Color.White;
                img = ImageProcessor.ScaleImage(img,
                                                128,
                                                64,
                                                ImageProcessor.ScalingTypes.FixedSize,
                                                Position,
                                                MarginTop,
                                                MarginBottom,
                                                MarginLeft,
                                                MarginRight,
                                                backgroundColor,
                                                InterpolationMode);
                _finalImage = ImageProcessor.MakeBinaryMatrix(img, UseHSL ? (HSLThreshold / 100) : RGBThreshold, false, UseHSL, ImageProcessor.DitheringAlgorithms.Binary);
                if (MergeWhites)
                {
                    _finalImage = ImageProcessor.MergeWhites(_currentImage, _finalImage);
                }
                else if (MergeBlacks)
                {
                    _finalImage = ImageProcessor.MergeBlacks(_currentImage, _finalImage);
                }
                ImagePreview.Source = BitmapToImageSource(ImageProcessor.MakeBinaryImage(_finalImage, img.Width, img.Height));
            }
            catch
            {
                // Ignore
            }
        }

        private void Checkbox_StateChanged(object sender, RoutedEventArgs e) => RefreshPreview();

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

        private void NumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e) => RefreshPreview();
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshPreview();
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => RefreshPreview();

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

        public bool[,] GetPixels() => _finalImage;

        private void Slider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => RefreshPreview();

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DrawTextWindow_OnSizeChanged(object sender, SizeChangedEventArgs e) => Trace.WriteLine($"Width: {ActualWidth}");
    }
}
