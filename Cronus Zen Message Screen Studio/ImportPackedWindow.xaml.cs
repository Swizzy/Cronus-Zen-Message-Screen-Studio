using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Image = System.Drawing.Image;

namespace CronusZenMessageScreenStudio
{
    /// <summary>
    ///     Interaction logic for LoadImage.xaml
    /// </summary>
    public partial class ImportPackedWindow
    {
        private bool[,] _finalImage;

        public ImportPackedWindow()
        {
            InitializeComponent();
            SizeChanged += (sender, args) => System.Diagnostics.Trace.WriteLine($"Width: {ActualWidth} Height: {ActualHeight}");
            LayoutRoot.DataContext = this;
        }

        public string InputText { get; set; }

        public bool[,] GetPixels() => _finalImage;

        private void Ok_click(object sender, RoutedEventArgs e)
        {
            try
            {
                _finalImage = TryParseInput(InputText);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool[,] TryParseInput(string inputText)
        {
            if (inputText.Any(c => c == '{' || c == '}'))
            {
                return ParsePacked(ExtractData(inputText));
            }
            return ParsePacked(inputText);
        }

        private string ExtractData(string inputText)
        {
            int dataStart = inputText.IndexOf('{');
            int dataEnd = inputText.IndexOf('}');
            return inputText.Substring(dataStart + 1, dataEnd - (dataStart + 1)).Trim();
        }

        private bool[,] ParsePacked(string inputText)
        {
            string[] parts = inputText.Split(',');
            if (parts.Length >= 3)
            {
                List<int> picture = new List<int>
                {
                    int.Parse(parts[0].Trim()), // Width
                    int.Parse(parts[1].Trim())  // Height
                };
                picture.AddRange(parts.Skip(2).Select(part => Convert.ToInt32(part.Trim(), 16)));
                int bits;
                switch (parts[2].Trim().Length)
                {
                    case 4:
                        bits = 8;
                        break;
                    case 6:
                        bits = 16;
                        break;
                    case 10:
                        bits = 32;
                        break;
                    default:
                        throw new InvalidDataException("Unable to determine the number of bits used");
                }
                return RenderImageAsBits(picture, bits);
            }
            throw new InvalidDataException("Unable to parse the data in the input field - unable to find atleast 3 values (width, height and a single packed entry)");
        }

        private bool[,] RenderImageAsBits(List<int> picture, int bits)
        {
            bool[,] pixels = new bool[128, 64];
            int pictureOffset = 1;
            int pictureBit = 0, pictureData = 0;
            int width = picture[1], height = picture[0];
            for (int pictureY = 0; pictureY < width; pictureY++)
            {
                for (int pictureX = 0; pictureX < height; pictureX++)
                {
                    if (pictureBit == 0)
                    {
                        pictureOffset++;
                        pictureData = picture[pictureOffset];
                        pictureBit = bits;
                    }
                    pictureBit--;
                    pixels[pictureX, pictureY] = TestBit(pictureData, pictureBit);
                }
            }
            return pixels;
        }

        private bool TestBit(int data, int bit)
        {
            int result = data & 1 << bit;
            return result != 0;
        }
    }
}
