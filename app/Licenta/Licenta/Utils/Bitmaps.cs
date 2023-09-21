using Licenta.Imaging;
using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Licenta.Commons.Utils;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Licenta.Utils
{
    public static class Bitmaps
    {
        public static Color ToColor(this Color24 c)
            => Color.FromArgb((byte)((double)c.R * 255), (byte)((double)c.G * 255), (byte)((double)c.B * 255));

        public static Bitmap ToBitmap(this Image24 image)
        {
            var colors = image.Items.Select(ToColor).Select(_ => _.ToArgb()).ToArray();
            var bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppRgb);
            var r = new Rectangle(Point.Empty, bitmap.Size);
            var data = bitmap.LockBits(r, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(colors, 0, data.Scan0, colors.Length);
            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static Color24[] GetColorsFromBitmap(this Bitmap bitmap)
        {
            var r = new Rectangle(Point.Empty, bitmap.Size);
            if (bitmap.PixelFormat == PixelFormat.Format32bppRgb || bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {                
                var data = bitmap.LockBits(r, ImageLockMode.ReadOnly, bitmap.PixelFormat);
                var ints = new int[data.Height * data.Width];
                Marshal.Copy(data.Scan0, ints, 0, ints.Length);
                bitmap.UnlockBits(data);
                return ints.Select(Color24.FromRGB).ToArray();
            }
            if(bitmap.PixelFormat == PixelFormat.Format24bppRgb)
            {                
                var data = bitmap.LockBits(r, ImageLockMode.ReadOnly, bitmap.PixelFormat);
                var bytes = new byte[data.Height * data.Width * 3];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                bitmap.UnlockBits(data);
                return bytes.GroupChunks(3).Select(c => new Color24(c[0], c[1], c[2])).ToArray();                
            }
            throw new InvalidOperationException($"Only 24bpp and 32bpp (A)RGB pixel formats are supported. Current Format = {bitmap.PixelFormat}");
        }
    }
}
