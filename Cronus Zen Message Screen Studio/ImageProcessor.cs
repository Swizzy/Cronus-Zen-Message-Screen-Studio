using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using static CronusZenMessageScreenStudio.ImageProcessor;

namespace CronusZenMessageScreenStudio
{
    public class ImageProcessor
    {
        public enum ScalingTypes
        {
            Scaled,
            FixedSize
        }

        [Flags]
        public enum Positions
        {
            Default = 0 << 0,
            HLeft   = 1 << 0,
            HCenter = 1 << 1,
            HRight  = 1 << 2,
            VTop    = 1 << 3,
            VCenter = 1 << 4,
            VBottom = 1 << 5,

            TopLeft   = VTop | HLeft,
            TopCenter = VTop | HCenter,
            TopRight  = VTop | HRight,

            CenterLeft  = VCenter | HLeft,
            Center      = VCenter | HCenter,
            CenterRight = VCenter | HRight,

            BottomLeft   = VBottom | HLeft,
            BottomCenter = VBottom | HCenter,
            BottomRight  = VBottom | HRight
        }

        public static List<SelectionData<Positions>> MakePositionSelectionList()
        {
            return new List<SelectionData<Positions>>
            {
                new SelectionData<Positions>("Top Left", Positions.TopLeft),
                new SelectionData<Positions>("Top Center", Positions.TopCenter),
                new SelectionData<Positions>("Top Right", Positions.TopRight),
                new SelectionData<Positions>("Center Left", Positions.CenterLeft),
                new SelectionData<Positions>("Center", Positions.Center),
                new SelectionData<Positions>("Center Right", Positions.CenterRight),
                new SelectionData<Positions>("Bottom Left", Positions.BottomLeft),
                new SelectionData<Positions>("Bottom Center", Positions.BottomCenter),
                new SelectionData<Positions>("Bottom Right", Positions.BottomRight)
            };
        }

        public enum DitheringAlgorithms
        {
            Binary,
            FloydSteinberg,
            JarvisJudiceNinke,
            Stucki,
            Atkinson,
            Burkes,
            Sierra,
            SierraTwoRow,
            SierraLite
        }

        public static IEnumerable MakeDitheringSelectionList()
        {
            yield return new SelectionData<DitheringAlgorithms>("Binary", DitheringAlgorithms.Binary);
            yield return new SelectionData<DitheringAlgorithms>("Floyd-Steinberg", DitheringAlgorithms.FloydSteinberg);
            yield return new SelectionData<DitheringAlgorithms>("Jarvis-Judice-Ninke", DitheringAlgorithms.JarvisJudiceNinke);
            yield return new SelectionData<DitheringAlgorithms>("Stucki", DitheringAlgorithms.Stucki);
            yield return new SelectionData<DitheringAlgorithms>("Atkinson", DitheringAlgorithms.Atkinson);
            yield return new SelectionData<DitheringAlgorithms>("Burkes", DitheringAlgorithms.Burkes);
            yield return new SelectionData<DitheringAlgorithms>("Sierra", DitheringAlgorithms.Sierra);
            yield return new SelectionData<DitheringAlgorithms>("Sierra Two Row", DitheringAlgorithms.SierraTwoRow);
            yield return new SelectionData<DitheringAlgorithms>("Sierra Lite", DitheringAlgorithms.SierraLite);
        }

        public static IEnumerable MakeInterpolationSelectionList()
        {
            return new List<SelectionData<InterpolationMode>>
            {
                new SelectionData<InterpolationMode>("Default", InterpolationMode.Default),
                new SelectionData<InterpolationMode>("Low quality", InterpolationMode.Low),
                new SelectionData<InterpolationMode>("High quality", InterpolationMode.High),
                new SelectionData<InterpolationMode>("Bilinear", InterpolationMode.Bilinear),
                new SelectionData<InterpolationMode>("Bicubic", InterpolationMode.Bicubic),
                new SelectionData<InterpolationMode>("Nearest neighbor", InterpolationMode.NearestNeighbor),
                new SelectionData<InterpolationMode>("High quality bilinear", InterpolationMode.HighQualityBilinear),
                new SelectionData<InterpolationMode>("High quality bicubic", InterpolationMode.HighQualityBicubic)
            };
        }

        public static IEnumerable MakePixelOffsetSelectionList()
        {
            yield return new SelectionData<PixelOffsetMode>("Default", PixelOffsetMode.Default);
            yield return new SelectionData<PixelOffsetMode>("None", PixelOffsetMode.None);
            yield return new SelectionData<PixelOffsetMode>("Half", PixelOffsetMode.Half);
            yield return new SelectionData<PixelOffsetMode>("High speed", PixelOffsetMode.HighSpeed);
            yield return new SelectionData<PixelOffsetMode>("High quality", PixelOffsetMode.HighQuality);
        }

        public static IEnumerable MakeSmoothingSelectionList()
        {
            yield return new SelectionData<SmoothingMode>("Default", SmoothingMode.Default);
            yield return new SelectionData<SmoothingMode>("None", SmoothingMode.None);
            yield return new SelectionData<SmoothingMode>("High speed", SmoothingMode.HighSpeed);
            yield return new SelectionData<SmoothingMode>("High quality", SmoothingMode.HighQuality);
            yield return new SelectionData<SmoothingMode>("Anti-alias", SmoothingMode.AntiAlias);
        }

        public static IEnumerable MakeCompositingQualitySelectionList()
        {
            yield return new SelectionData<CompositingQuality>("Default", CompositingQuality.Default);
            yield return new SelectionData<CompositingQuality>("High speed", CompositingQuality.HighSpeed);
            yield return new SelectionData<CompositingQuality>("High quality", CompositingQuality.HighQuality);
            yield return new SelectionData<CompositingQuality>("Gamma Corrected", CompositingQuality.GammaCorrected);
            yield return new SelectionData<CompositingQuality>("Assume linear", CompositingQuality.AssumeLinear);
        }

        public static Bitmap LoadImage(string filename) => new Bitmap(filename);

        public static int GetFrameCount(Bitmap img)
        {
            if (img.RawFormat.Equals(ImageFormat.Gif) && img.FrameDimensionsList.Contains(FrameDimension.Time.Guid))
            {
                return img.GetFrameCount(FrameDimension.Time) - 1;
            }
            return 0;
        }

        public static Bitmap SetFrame(Bitmap img, int frame)
        {
            var toReturn = (Bitmap)img.Clone();
            if (img.RawFormat.Equals(ImageFormat.Gif) && img.FrameDimensionsList.Contains(FrameDimension.Time.Guid))
            {
                if (toReturn.GetFrameCount(FrameDimension.Time) > frame)
                {
                    toReturn.SelectActiveFrame(FrameDimension.Time, frame);
                }
                else
                {
                    toReturn.SelectActiveFrame(FrameDimension.Time, toReturn.GetFrameCount(FrameDimension.Time) - 1);
                }
            }

            return toReturn;
        }

        private static byte[,,] ReadBitmapToColorBytes(Bitmap bitmap)
        {
            byte[,,] bytes = new byte[bitmap.Width, bitmap.Height, 3];
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    bytes[x, y, 0] = pixel.R;
                    bytes[x, y, 1] = pixel.G;
                    bytes[x, y, 2] = pixel.B;
                }
            }
            return bytes;
        }

        public static Bitmap ScaleImage(Image input,
                                        int width,
                                        int height,
                                        ScalingTypes scalingType,
                                        Positions position,
                                        int marginTop,
                                        int marginBottom,
                                        int marginLeft,
                                        int marginRight,
                                        Color backgroundColor,
                                        InterpolationMode interpolationMode)
        {
            int sourceWidth = input.Width;
            int sourceHeight = input.Height;
            int destWidth, targetWidth;
            int destHeight, targetHeight;
            int destX = 0;
            int destY = 0;

            if (scalingType == ScalingTypes.Scaled)
            {
                double maxRatio = width / (double)height;
                double srcRatio = sourceWidth / (double)sourceHeight;
                if (srcRatio > maxRatio)
                {
                    destWidth = targetWidth = width;
                    destHeight = targetHeight = (int)(height / srcRatio);
                }
                else
                {
                    destWidth = targetWidth = (int)(width / srcRatio);
                    destHeight = targetHeight = height;
                }
            }
            else
            {
                targetWidth = width;
                targetHeight = height;

                width -= marginLeft + marginRight;
                height -= marginTop + marginBottom;

                double ratio = Math.Min(width / (double)sourceWidth, height / (double)sourceHeight);
                destWidth = (int)(sourceWidth * ratio);
                destHeight = (int)(sourceHeight * ratio);

                if (width > destWidth)
                {
                    if ((position & Positions.HCenter) == Positions.HCenter)
                    {
                        destX = Convert.ToInt32((targetWidth - (destWidth + marginLeft + marginRight)) / 2);
                    }
                    else if ((position & Positions.HRight) == Positions.HRight)
                    {
                        destX = targetWidth - destWidth;
                    }
                }
                if (height > destHeight)
                {
                    if ((position & Positions.VCenter) == Positions.VCenter)
                    {
                        destY = Convert.ToInt32((targetHeight - (destHeight + marginTop + marginBottom)) / 2);
                    }
                    else if ((position & Positions.VBottom) == Positions.VBottom)
                    {
                        destY = targetHeight - destHeight;
                    }
                }
                destX += marginLeft;
                destY += marginTop;
            }

            PixelFormat pixelFormat = PixelFormat.Format24bppRgb;
            if (backgroundColor == Color.Transparent)
            {
                pixelFormat = PixelFormat.Format32bppArgb;
            }
            Bitmap toReturn = new Bitmap(targetWidth, targetHeight, pixelFormat);
            toReturn.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using (Graphics graphics = Graphics.FromImage(toReturn))
            {
                graphics.Clear(backgroundColor);
                graphics.InterpolationMode = interpolationMode;
                graphics.DrawImage(input, new Rectangle(destX, destY, destWidth, destHeight), new Rectangle(0, 0, sourceWidth, sourceHeight), GraphicsUnit.Pixel);
            }
            return toReturn;
        }


        public static Bitmap FlipImage(Bitmap input, bool flipHorizontal, bool flipVertical)
        {
            if (!flipHorizontal && !flipVertical)
            {
                return input;
            }

            var flipType = flipHorizontal switch
                           {
                               true when flipVertical => RotateFlipType.RotateNoneFlipXY, // Flip both Vertical and Horizontal at the same time
                               true                   => RotateFlipType.RotateNoneFlipX,  // Flip just Horizontal
                               _                      => RotateFlipType.RotateNoneFlipY   // Flip just Vertical
                           };

            var flippedImage = (Bitmap)input.Clone();
            flippedImage.RotateFlip(flipType);
            return flippedImage;
        }

        public static Bitmap RotateImage(Bitmap input, double angleDegrees, InterpolationMode interpolationMode, PixelOffsetMode pixelOffsetMode, SmoothingMode smoothingMode, CompositingQuality compositingQuality, Color backgroundColor)
        {
            if (Math.Abs(angleDegrees) < 0.01) // If no rotation is needed, just pass it back as-is
            {
                return input;
            }

            var angleRadians = angleDegrees * (Math.PI / 180);
            var cos = Math.Abs(Math.Cos(angleRadians));
            var sin = Math.Abs(Math.Sin(angleRadians));

            var srcWidth = input.Width;
            var srcHeight = input.Height;

            var newWidth = (int)Math.Round(srcWidth * cos + srcHeight * sin);
            var newHeight = (int)Math.Round(srcHeight * cos + srcWidth * sin);

            var rotatedBmp = new Bitmap(newWidth, newHeight, input.PixelFormat);
            rotatedBmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);

            using (var g = Graphics.FromImage(rotatedBmp))
            {
                g.InterpolationMode = interpolationMode;
                g.PixelOffsetMode = pixelOffsetMode;
                g.SmoothingMode = smoothingMode;
                g.CompositingQuality = compositingQuality;

                g.Clear(backgroundColor);

                // Set the rotation point to the center of the image
                g.TranslateTransform(newWidth / 2f, newHeight / 2f);

                // Rotate
                g.RotateTransform((float)angleDegrees);

                // Move the image back
                g.TranslateTransform(-srcWidth / 2f, -srcHeight / 2f);

                // Draw the rotated image
                g.DrawImage(input, new PointF(0, 0));
            }

            return rotatedBmp;
        }

        public static bool[,] MakeBinaryMatrix(Bitmap img, double threshold, bool invert, bool useHSL, DitheringAlgorithms algorithm)
        {
            var colorFunction = CreateColorFunction(threshold, invert, useHSL);

            // Apply dithering
            DitheringBase<byte> dithering = algorithm switch
            {
                DitheringAlgorithms.Atkinson => new AtkinsonDitheringRGB<byte>(colorFunction),
                DitheringAlgorithms.Burkes => new BurkesDitheringRGB<byte>(colorFunction),
                DitheringAlgorithms.FloydSteinberg => new FloydSteinbergDitheringRGB<byte>(colorFunction),
                DitheringAlgorithms.JarvisJudiceNinke => new JarvisJudiceNinkeDitheringRGB<byte>(colorFunction),
                DitheringAlgorithms.Stucki => new StuckiDitheringRGB<byte>(colorFunction),
                DitheringAlgorithms.Sierra => new SierraDitheringRGB<byte>(colorFunction),
                DitheringAlgorithms.SierraLite => new SierraLiteDitheringRGB<byte>(colorFunction),
                DitheringAlgorithms.SierraTwoRow => new SierraTwoRowDitheringRGB<byte>(colorFunction),
                DitheringAlgorithms.Binary => new FakeDitheringRGB<byte>(colorFunction),
                _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
            };

            byte[,,] bytes = ReadBitmapToColorBytes(img);
            TempByteImageFormat temp = new TempByteImageFormat(bytes);
            dithering.DoDithering(temp);

            // Convert dithered result to bool matrix
            bool[,] result = new bool[img.Width, img.Height];
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    byte[] rgb = temp.GetPixelChannels(x, y);
                    result[x, y] = rgb[0] == 255; // true if white, false if black
                }
            }

            return result;
        }

        private static DitheringBase<byte>.ColorFunction CreateColorFunction(double threshold, bool invert, bool useHSL)
        {
            return (in byte[] input, ref byte[] output) =>
            {
                double avg;
                if (useHSL)
                {
                    Color pixel = Color.FromArgb(input[0], input[1], input[2]);
                    avg = pixel.GetBrightness();
                }
                else
                {
                    // RGB average
                    avg = (input[0] + input[1] + input[2]) / 3.0;
                }

                bool shouldBeWhite = invert ? avg < threshold : avg >= threshold;
                byte value = (byte)(shouldBeWhite ? 255 : 0);

                output[0] = value; // R
                output[1] = value; // G
                output[2] = value; // B
            };
        }

        public static Bitmap MakeBinaryImage(bool[,] pixels, int width, int height)
        {
            Bitmap toReturn = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    toReturn.SetPixel(x, y, pixels[x, y] ? Color.White : Color.Black);
                }
            }
            return toReturn;
        }

        public static Bitmap MakeImage(Color[,] pixels, int width, int height)
        {
            Bitmap toReturn = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    toReturn.SetPixel(x, y, pixels[x, y]);
                }
            }
            return toReturn;
        }

        public static string GetFilterString()
        {
            List<string> fileFormats = new List<string>();
            StringBuilder toReturn = new StringBuilder();
            string sep = "";
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
            {
                string name = codec.CodecName?.Replace("Built-in", "").Replace("Codec", "Files").Trim() ?? $"{codec.FilenameExtension} Files";
                string ext = codec.FilenameExtension;
                toReturn.AppendFormat("{0}{1} ({2})|{2}", sep, name, ext);
                sep = "|";
                fileFormats.Add(ext);
            }
            toReturn.Append(sep + "All files (*.*)|*.*");

            string extensions = string.Join(";", fileFormats);
            return $"All supported image files ({extensions})|{extensions}|{toReturn}";
        }

        public static Bitmap DrawText(string text, Font font, bool whiteOnBlack)
        {
            Bitmap toReturn = new Bitmap(1, 1);
            int width, height;
            using (Graphics graphics = Graphics.FromImage(toReturn))
            {
                SizeF textSize = graphics.MeasureString(text, font);
                width = (int)Math.Ceiling(textSize.Width);
                height = (int)Math.Ceiling(textSize.Height);
            }
            toReturn = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            using (Graphics graphics = Graphics.FromImage(toReturn))
            {
                Brush foreground = whiteOnBlack ? Brushes.White : Brushes.Black;
                Color background = whiteOnBlack ? Color.Black : Color.White;
                graphics.Clear(background);
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawString(text, font, foreground, 0, 0);
            }
            return toReturn;
        }

        public static bool[,] MergeWhites(bool[,] current, bool[,] toMerge)
        {
            var toReturn = new bool[128, 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    toReturn[x, y] = current[x, y] || toMerge[x, y];
                }
            }

            return toReturn;
        }

        public static bool[,] MergeBlacks(bool[,] current, bool[,] toMerge)
        {
            var toReturn = new bool[128, 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    if (toMerge[x, y])
                    {
                        toReturn[x, y] = current[x, y];
                    }
                    else
                    {
                        toReturn[x, y] = toMerge[x, y];
                    }
                }
            }

            return toReturn;
        }
    }
}
