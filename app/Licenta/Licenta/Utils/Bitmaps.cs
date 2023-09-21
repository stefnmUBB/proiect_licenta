using Licenta.Imaging;
using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Licenta.Utils
{
    public static class Bitmaps
    {
        public static Color24[] GetColorsFromBitmap(this Bitmap bitmap)
        {
            if (bitmap.PixelFormat == PixelFormat.Format32bppRgb || bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {
                var r = new Rectangle(Point.Empty, bitmap.Size);
                var data = bitmap.LockBits(r, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                var ints = new int[data.Height * data.Width];
                Marshal.Copy(data.Scan0, ints, 0, ints.Length);
                return ints.Select(Color24.FromRGB).ToArray();
            }
            throw new InvalidOperationException("Only 32bpp (A)RGB pixel formats are supported");
        }
    }
}
