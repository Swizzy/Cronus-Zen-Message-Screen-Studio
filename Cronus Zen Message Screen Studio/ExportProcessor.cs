using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace CronusZenMessageScreenStudio
{
    internal class ExportProcessor
    {
        [Flags]
        internal enum ExportSettings
        {
            ForceWhite       = 1 << 0,
            ForceBlack       = 1 << 1,
            IndividualPixels = 1 << 2,
            Packed16Bit      = 1 << 3,
            Packed8Bit       = 1 << 4,
            Packed1DArray    = 1 << 5,
            FixedWidth       = 1 << 6,
            IncludeClear     = 1 << 7,
            IncludeFunction  = 1 << 8,
            SampleScript     = 1 << 9,

            Packed = Packed8Bit | Packed16Bit
        }

        private readonly List<PixelControl> _pixelControls;
        public ExportProcessor(List<PixelControl> pixelControls) { _pixelControls = pixelControls; }

        public void Savefile(string data)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = ".gpc";
            sfd.FileName = "script.gpc";
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, data);
            }
        }

        public string GenerateExportData(ExportSettings settings, string identifier)
        {
            StringBuilder toReturn = new StringBuilder();
            bool whitePixels = (settings & ExportSettings.ForceWhite) == ExportSettings.ForceWhite;
            if (whitePixels == false)
            {
                if ((settings & ExportSettings.ForceBlack) != ExportSettings.ForceBlack)
                {
                    whitePixels = WhitePixelMinority;
                }
            }

            if ((settings & ExportSettings.Packed) != 0)
            {
                toReturn.AppendLine(GeneratePacked(settings, identifier, whitePixels));
            }

            if ((settings & ExportSettings.SampleScript) == ExportSettings.SampleScript)
            {
                toReturn.AppendLine("main {");
                toReturn.AppendLine("\tif (get_val(XB1_A)) {");
                if ((settings & ExportSettings.Packed) != 0)
                {
                    toReturn.AppendLine($"\t\tdraw_{identifier}(0, 0, 0);");
                    toReturn.AppendLine("\t}");
                    toReturn.AppendLine("\telse if (get_val(XB1_B)) {");
                    toReturn.AppendLine($"\t\tdraw_{identifier}(0, 0, 1);");
                }
            }

            if ((settings & ExportSettings.IndividualPixels) == ExportSettings.IndividualPixels)
            {
                bool includeClear = (settings & ExportSettings.IncludeClear) == ExportSettings.IncludeClear;
                toReturn.AppendLine(GeneratePixelOledLines(includeClear, whitePixels, 2));
            }

            if ((settings & ExportSettings.SampleScript) == ExportSettings.SampleScript)
            {
                toReturn.AppendLine("\t}");
                toReturn.AppendLine("}");
            }

            if ((settings & ExportSettings.Packed) != 0 &&
                (settings & (ExportSettings.IncludeFunction | ExportSettings.SampleScript)) != 0)
            {
                toReturn.AppendLine(GeneratePackedFunction(settings, identifier));
            }
            return toReturn.ToString().Trim();
        }

        private string GeneratePackedFunction(ExportSettings settings, string identifier)
        {
            var toReturn = new StringBuilder();
            toReturn.AppendLine($"int __{identifier}Width, __{identifier}X, __{identifier}X2, __{identifier}Height, __{identifier}Y, __{identifier}Y2, __{identifier}Bit, __{identifier}Offset, __{identifier}Data;");
            toReturn.AppendLine($"function draw_{identifier}(x, y, invert) {{");
            if ((settings & ExportSettings.Packed1DArray) == ExportSettings.Packed1DArray)
            {
                int bits = 0;
                if ((settings & ExportSettings.Packed8Bit) == ExportSettings.Packed8Bit)
                {
                    bits = 8;
                }
                else if ((settings & ExportSettings.Packed16Bit) == ExportSettings.Packed16Bit)
                {
                    bits = 16;
                }
                toReturn.AppendLine($"\t__{identifier}Offset = 1; // Reset the starting point");
                toReturn.AppendLine($"\t__{identifier}Bit = 0; // Reset bit flag");
                toReturn.AppendLine($"\t__{identifier}Width = {identifier}[0]; // Fetch the width of what to draw");
                toReturn.AppendLine($"\t__{identifier}Height = {identifier}[1]; // Fetch the height of what to draw");
                toReturn.AppendLine($"\tfor (__{identifier}Y = 0; __{identifier}Y < __{identifier}Height; __{identifier}Y++) {{ // Loop the Y axis");
                toReturn.AppendLine($"\t\t__{identifier}Y2 = y + __{identifier}Y;");
                toReturn.AppendLine($"\t\tif (__{identifier}Y2 < 0 || __{identifier}Y2 >= 64) {{");
                toReturn.AppendLine($"\t\t\t__{identifier}Y2 -= 64;");
                toReturn.AppendLine("\t\t}");
                toReturn.AppendLine($"\t\tfor (__{identifier}X = 0; __{identifier}X < __{identifier}Width; __{identifier}X++) {{ // Loop the X axis");
                toReturn.AppendLine($"\t\t\tif (!__{identifier}Bit) {{ // Check if we've already handled the last bit");
                toReturn.AppendLine($"\t\t\t\t__{identifier}Bit = {bits}; // Reset the bit flag");
                toReturn.AppendLine($"\t\t\t\t__{identifier}Offset++; // Move to the next value");
                toReturn.AppendLine($"\t\t\t\t__{identifier}Data = {identifier}[__{identifier}Offset]; // Fetch the value");
                toReturn.AppendLine("\t\t\t}");
                toReturn.AppendLine($"\t\t\t__{identifier}Bit--; // Decrement the bit flag, we're moving to the next bit");
                toReturn.AppendLine($"\t\t\t__{identifier}X2 = x + __{identifier}X;");
                toReturn.AppendLine($"\t\t\tif (__{identifier}X2 < 0 || __{identifier}X2 >= 128) {{");
                toReturn.AppendLine($"\t\t\t\t__{identifier}X2 -= 128;");
                toReturn.AppendLine("\t\t\t}");
                toReturn.AppendLine($"\t\t\tif (test_bit(__{identifier}Data, __{identifier}Bit)) {{");
                toReturn.AppendLine($"\t\t\t\tpixel_oled(__{identifier}X2, __{identifier}Y2, !invert);");
                toReturn.AppendLine("\t\t\t}");
                toReturn.AppendLine("\t\t\telse {");
                toReturn.AppendLine($"\t\t\t\tpixel_oled(__{identifier}X2, __{identifier}Y2, invert);");
                toReturn.AppendLine("\t\t\t}");
                toReturn.AppendLine("\t\t}");
                toReturn.AppendLine("\t}");
                toReturn.AppendLine("}");
            }
            return toReturn.ToString().Trim();
        }

        private string GeneratePacked(ExportSettings settings, string identifier, bool whitePixels)
        {
            var toReturn = new StringBuilder();
            bool isFixed = (settings & ExportSettings.FixedWidth) == ExportSettings.FixedWidth;
            if ((settings & ExportSettings.Packed1DArray) == ExportSettings.Packed1DArray)
            {
                int dwidth = 0, dheight = 0;
                var dataType = "";
                var dataString = "";
                if ((settings & ExportSettings.Packed8Bit) == ExportSettings.Packed8Bit)
                {
                    (int width, int height, byte[] data) = Pack8(isFixed, whitePixels);
                    dwidth = width;
                    dheight = height;
                    dataType = "byte";
                    dataString = string.Join(",", data.Select(d => $" 0x{d:X02}")).Trim();
                }
                else if ((settings & ExportSettings.Packed16Bit) == ExportSettings.Packed16Bit)
                {
                    (int width, int height, ushort[] data) = Pack16(isFixed, whitePixels);
                    dwidth = width;
                    dheight = height;
                    dataType = "int";
                    dataString = string.Join(",", data.Select(d => $" 0x{d:X04}")).Trim();
                }
                toReturn.AppendLine($"const {dataType} {identifier}[] = {{{dwidth}, {dheight}, {dataString}}};");
            }
            return toReturn.ToString();
        }

        private bool WhitePixelMinority
        {
            get
            {
                int whitePixels = _pixelControls.Count(p => p.Color);
                int blackPixels = _pixelControls.Count - whitePixels;
                return blackPixels >= whitePixels;
            }
        }

        private (int width, int height, bool[,] matrix) GetPixelMatrix(bool allPixels, bool whitePixels)
        {
            int width;
            int height;
            if (allPixels)
            {
                width = _pixelControls.Max(p => p.X) + 1;
                height = _pixelControls.Max(p => p.Y) + 1;
            }
            else
            {
                List<PixelControl> pixels = _pixelControls.Where(p => p.Color == whitePixels)
                                                          .DefaultIfEmpty(new PixelControl())
                                                          .ToList();
                width = pixels.Max(p => p.X) + 1;
                height = pixels.Max(p => p.Y) + 1;
            }
            bool[,] matrix = new bool[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    matrix[x, y] = _pixelControls.First(p => p.X == x && p.Y == y).Color;
                }
            }

            return (width, height, matrix);
        }

        private (int width, int height, Color[,] matrix) GetPixelColors(bool allPixels, bool whitePixels)
        {
            int width;
            int height;
            if (allPixels)
            {
                width = _pixelControls.Max(p => p.X) + 1;
                height = _pixelControls.Max(p => p.Y) + 1;
            }
            else
            {
                List<PixelControl> pixels = _pixelControls.Where(p => p.Color == whitePixels)
                                                          .DefaultIfEmpty(new PixelControl())
                                                          .ToList();
                width = pixels.Max(p => p.X) + 1;
                height = pixels.Max(p => p.Y) + 1;
            }
            Color[,] matrix = new Color[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    matrix[x, y] = _pixelControls.First(p => p.X == x && p.Y == y).GetColor();
                }
            }

            return (width, height, matrix);
        }

        private (int width, int height, byte[] data) Pack8(bool allPixels, bool whitePixels)
        {
            (int width, int height, bool[,] matrix) = GetPixelMatrix(allPixels, whitePixels);
            List<byte> data = new List<byte>();
            int currentValue = 0;
            int bit = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    currentValue <<= 1; // Shift it left
                    if (matrix[x, y])
                    {
                        currentValue |= 1;
                    }
                    bit++; // Increment the bit counter, we're moving on to the next bit now...
                    if (bit == 8)
                    {
                        data.Add((byte)currentValue);
                        currentValue = 0;
                        bit = 0;
                    }
                }
            }
            // Make sure we add the last packed byte
            if (bit > 0)
            {
                data.Add((byte)(currentValue << 8 - bit));
            }
            return (width, height, data.ToArray());
        }

        private (int width, int height, ushort[] data) Pack16(bool allPixels, bool whitePixels)
        {
            (int width, int height, bool[,] matrix) = GetPixelMatrix(allPixels, whitePixels);
            List<ushort> data = new List<ushort>();
            int currentValue = 0;
            int bit = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    currentValue <<= 1; // Shift it left
                    if (matrix[x, y])
                    {
                        currentValue |= 1;
                    }
                    bit++; // Increment the bit counter, we're moving on to the next bit now...
                    if (bit == 16)
                    {
                        data.Add((ushort)currentValue);
                        currentValue = 0;
                        bit = 0;
                    }
                }
            }
            // Make sure we add the last packed 16-bit value
            if (bit > 0)
            {
                data.Add((ushort)(currentValue << 16 - bit));
            }
            return (width, height, data.ToArray());
        }

        private string GeneratePixelOledLines(bool includeClear, bool whitePixels, int indentationLevel)
        {
            StringBuilder data = new StringBuilder();
            if (includeClear)
            {
                data.AppendLine(new string('\t', indentationLevel) + $"cls_oled({(whitePixels ? 0 : 1)});");
            }
            foreach (PixelControl pixel in _pixelControls.Where(p => p.Color == whitePixels))
            {
                data.AppendLine(GeneratePixelOled(pixel, new string('\t', indentationLevel)));
            }

            return data.ToString().Trim();
        }

        private string GeneratePixelOled(PixelControl pixel, string prefix) => $"{prefix}pixel_oled({pixel.X}, {pixel.Y}, {(pixel.Color ? 1 : 0)});";

        public Image GenerateImage()
        {
            (int width, int height, bool[,] matrix) = GetPixelMatrix(true, true);
            Bitmap img = ImageProcessor.MakeBinaryImage(matrix, width, height);
            return img;
        }

        public Image GenerateColoredImage()
        {
            (int width, int height, Color[,] pixels) = GetPixelColors(true, true);
            Bitmap img = ImageProcessor.MakeImage(pixels, width, height);
            return img;
        }

        public void GenerateAndSaveImage()
        {
            Image img = GenerateImage();
            SaveFileDialog sfd = new SaveFileDialog()
                      {
                          FileName = "image.bmp",
                          AddExtension = true,
                          DefaultExt = ".bmp"
                      };
            StringBuilder filters = new StringBuilder();
            filters.AppendFormat("Bitmap Images ({0})|{0}", "*.bmp");
            filters.AppendFormat("|JPEG Images ({0})|{0}", "*.jpg;jpeg");
            filters.AppendFormat("|GIF Images ({0})|{0}", "*.gif");
            filters.AppendFormat("|TIFF Images ({0})|{0}", "*.tiff");
            filters.AppendFormat("|PNG Images ({0})|{0}", "*.png");
            sfd.Filter = filters.ToString();

            if (sfd.ShowDialog() == true)
            {
                switch (sfd.FilterIndex)
                {
                    case 1:
                        img.Save(sfd.FileName, ImageFormat.Bmp);
                        break;
                    case 2:
                        img.Save(sfd.FileName, ImageFormat.Jpeg);
                        break;
                    case 3:
                        img.Save(sfd.FileName, ImageFormat.Gif);
                        break;
                    case 4:
                        img.Save(sfd.FileName, ImageFormat.Tiff);
                        break;
                    case 5:
                        img.Save(sfd.FileName, ImageFormat.Png);
                        break;
                }
            }
        }
    }
}
