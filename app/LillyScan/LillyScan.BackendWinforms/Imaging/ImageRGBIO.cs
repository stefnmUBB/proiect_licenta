using LillyScan.Backend.Math;
using LillyScan.BackendWinforms.Utils;
using System.Drawing;

namespace LillyScan.Backend.Imaging
{
    public static class ImageRGBIO
    {
        public static ImageRGB FromBitmap(Bitmap bitmap) 
            => new ImageRGB(new Matrix<ColorRGB>(bitmap.Height, bitmap.Width, bitmap.GetColorsFromBitmap()));

        public static ImageRGB FromFile(string path, float percentage)
        {
            using (var bmp = new Bitmap(path))
            {
                var newWidth = (int)System.Math.Max(1, bmp.Width * percentage / 100);
                var newHeight = (int)System.Math.Max(1, bmp.Height * percentage / 100);
                using (var bmpformatted = new Bitmap(newWidth, newHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
                {
                    using (var g = Graphics.FromImage(bmpformatted))
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        g.DrawImage(bmp, 0, 0, newWidth, newHeight);
                    }
                    return FromBitmap(bmpformatted);
                }
            }
        }

        public static ImageRGB FromFile(string path, int newWidth, int newHeight)
        {
            using (var bmp = new Bitmap(path))
            using (var bmpformatted = new Bitmap(newWidth, newHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
            {
                Graphics.FromImage(bmpformatted).DrawImage(bmp, 0, 0, newWidth, newHeight);
                return FromBitmap(bmpformatted);
            }
        }
    }
}
