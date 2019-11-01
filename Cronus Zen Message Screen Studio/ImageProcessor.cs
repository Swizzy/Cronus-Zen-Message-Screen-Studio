using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace CronusZenMessageScreenStudio
{
    internal class ImageProcessor
    {
        public static Bitmap LoadImage(string filename) => new Bitmap(filename);

        public static Bitmap ScaleImage(Image input, int width, int height)
        {
            int sourceWidth = input.Width;
            int sourceHeight = input.Height;
            double ratio = Math.Min(width / (double)sourceWidth, height / (double)sourceHeight);
            int destX = 0;
            int destY = 0;
            int destWidth = (int)(sourceWidth * ratio);
            int destHeight = (int)(sourceHeight * ratio);
            if (width > destWidth)
            {
                destX = Convert.ToInt32((width - destWidth) / 2);
            }
            if (height > destHeight)
            {
                destY = Convert.ToInt32((height - destHeight) / 2);
            }
            Bitmap toReturn = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            toReturn.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using (Graphics graphics = Graphics.FromImage(toReturn))
            {
                graphics.Clear(Color.Black);
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(input, new Rectangle(destX, destY, destWidth, destHeight), new Rectangle(0, 0, sourceWidth, sourceHeight), GraphicsUnit.Pixel);
            }
            return toReturn;
        }

        public static bool[,] MakeBinaryMatrix(Bitmap img)
        {
            var toReturn = new bool[img.Width,img.Height];
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    var pixel = img.GetPixel(x, y);
                    var sum = new[] { pixel.R, pixel.G, pixel.B }.Sum(c => c);
                    toReturn[x, y] = sum >= 1;
                }
            }

            return toReturn;
        }

        public static string GetFilterString()
        {
            var toReturn = new StringBuilder();
            var sep = "";
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
            {
                var name = codec.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                var ext = codec.FilenameExtension;
                toReturn.AppendFormat("{0}{1} ({2})|{2}", sep, name, ext);
                sep = "|";
            }
            toReturn.Append(sep + "All files (*.*)|*.*");
            return toReturn.ToString();
        }
    }
}
