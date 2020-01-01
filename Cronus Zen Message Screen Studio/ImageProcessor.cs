using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

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
            Default    = 0 << 0,
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

        public static Bitmap LoadImage(string filename) => new Bitmap(filename);

        public static Bitmap ScaleImage(Image input, int width, int height)
        {
            return ScaleImage(input,
                              width,
                              height,
                              ScalingTypes.Scaled,
                              Positions.Default,
                              0,
                              0,
                              0,
                              0,
                              Color.Transparent);
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
                                        Color backgroundColor)
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
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(input, new Rectangle(destX, destY, destWidth, destHeight), new Rectangle(0, 0, sourceWidth, sourceHeight), GraphicsUnit.Pixel);
            }
            return toReturn;
        }

        public static bool[,] MakeBinaryMatrix(Bitmap img, double threshold, bool invert)
        {
            bool[,] toReturn = new bool[img.Width,img.Height];
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Color pixel = img.GetPixel(x, y);
                    List<double> colors = new List<double> { pixel.R, pixel.G, pixel.B };
                    double avg = colors.Average(c => c);
                    if (invert)
                    {
                        toReturn[x, y] = avg < threshold;
                    }
                    else
                    {
                        toReturn[x, y] = avg >= threshold;
                    }
                }
            }

            return toReturn;
        }

        public static Bitmap MakeBinaryImage(Bitmap img, double threshold, bool invert)
        {
            bool[,] pixels = MakeBinaryMatrix(img, threshold, invert);
            return MakeBinaryImage(pixels, img.Width, img.Height);
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
                string name = codec.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                string ext = codec.FilenameExtension;
                toReturn.AppendFormat("{0}{1} ({2})|{2}", sep, name, ext);
                sep = "|";
                fileFormats.Add(ext);
            }
            toReturn.Append(sep + "All files (*.*)|*.*");

            string extensions = string.Join(";", fileFormats);
            return $"All Imagefiles ({extensions})|{extensions}|{toReturn}";
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
    }
}
