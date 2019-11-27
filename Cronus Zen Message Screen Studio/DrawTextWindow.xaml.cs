using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
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
    public partial class DrawTextWindow
    {
        private Bitmap _finalImage;

        public DrawTextWindow()
        {
            InitializeComponent();
            SizeChanged += (sender, args) => System.Diagnostics.Trace.WriteLine($"Width: {ActualWidth} Height: {ActualHeight}");
            LayoutRoot.DataContext = this;
            TextFontSize = 20;
            PositionBox.Items.Add(new LoadImageWindow.PositionSelection("Top Left", ImageProcessor.Positions.TopLeft));
            PositionBox.Items.Add(new LoadImageWindow.PositionSelection("Top Center", ImageProcessor.Positions.TopCenter));
            PositionBox.Items.Add(new LoadImageWindow.PositionSelection("Top Right", ImageProcessor.Positions.TopRight));
            PositionBox.Items.Add(new LoadImageWindow.PositionSelection("Center Left", ImageProcessor.Positions.CenterLeft));
            PositionBox.Items.Add(new LoadImageWindow.PositionSelection("Center", ImageProcessor.Positions.Center));
            PositionBox.Items.Add(new LoadImageWindow.PositionSelection("Center Right", ImageProcessor.Positions.CenterRight));
            PositionBox.Items.Add(new LoadImageWindow.PositionSelection("Bottom Left", ImageProcessor.Positions.BottomLeft));
            PositionBox.Items.Add(new LoadImageWindow.PositionSelection("Bottom Center", ImageProcessor.Positions.BottomCenter));
            PositionBox.Items.Add(new LoadImageWindow.PositionSelection("Bottom Right", ImageProcessor.Positions.BottomRight));

            using (var installedFontCollection = new InstalledFontCollection())
            {
                foreach (FontFamily fontFamily in installedFontCollection.Families)
                {
                    FontBox.Items.Add(fontFamily);
                }
            }
            TextFontFamily = (FontFamily)FontBox.Items[0];
            Threshold = 200;
            MarginTop = 0;
            MarginBottom = 0;
            MarginLeft = 0;
            MarginRight = 0;
            Position = ImageProcessor.Positions.Center;
            WhiteOnBlack = true;
        }

        public int Threshold { get; set; }
        public int MarginTop { get; set; }
        public int MarginBottom { get; set; }
        public int MarginLeft { get; set; }
        public int MarginRight { get; set; }
        public ImageProcessor.Positions Position { get; set; }

        public FontFamily TextFontFamily { get; set; }
        public int TextFontSize { get; set; }
        public bool BoldFont { get; set; }
        public bool ItalicFont { get; set; }
        public bool UnderlineFont { get; set; }
        public bool StrikeoutFont { get; set; }

        public bool WhiteOnBlack { get; set; }

        private void RefreshPreview()
        {
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
                                                backgroundColor);
                _finalImage = ImageProcessor.MakeBinaryImage(img, Threshold, false);
                ImagePreview.Source = BitmapToImageSource(_finalImage);
            }
            catch
            {
                // Ignore
            }
        }

        private void Checkbox_StateChanged(object sender, RoutedEventArgs e) => RefreshPreview();
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

        public bool[,] GetPixels() => ImageProcessor.MakeBinaryMatrix(_finalImage, Threshold, false);
    }
}
