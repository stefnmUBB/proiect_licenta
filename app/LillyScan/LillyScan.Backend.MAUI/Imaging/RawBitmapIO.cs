#if ANDROID
using Android.Graphics;
#endif
using LillyScan.Backend.Imaging;
using Microsoft.Maui.Graphics.Platform;

namespace LillyScan.Backend.MAUI.Imaging
{
    public static class RawBitmapIO
    {
        public static unsafe RawBitmap FromBitmap(Microsoft.Maui.Graphics.IImage image)
        {
#if ANDROID
            var bmp = image.AsBitmap();
            var buffer = new int[bmp.Width * bmp.Height];
            bmp.GetPixels(buffer, 0, bmp.Width, 0, 0, bmp.Width, bmp.Height);
            return RawBitmaps.FromRGB(bmp.Width, bmp.Height, buffer);            
#else
            throw new NotImplementedException();
#endif

        }

        public static unsafe Microsoft.Maui.Graphics.IImage ToImage(this RawBitmap bmp)
        {
#if ANDROID
            int[] colors = bmp.ToRGB();            
            return new PlatformImage(Bitmap.CreateBitmap(colors, bmp.Width, bmp.Height, Bitmap.Config.Argb8888));
#else
            throw new NotImplementedException();    
#endif
        }
    }
}
