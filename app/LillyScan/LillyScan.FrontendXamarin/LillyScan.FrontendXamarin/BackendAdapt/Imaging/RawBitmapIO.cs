using Android.Graphics;
using LillyScan.Backend.Imaging;

namespace LillyScan.FrontendXamarin.BackendAdapt.Imaging
{
    public static class RawBitmapIO
    {
        public static RawBitmap FromAndroidBitmap(Bitmap bmp)
        {
            var buffer = new int[bmp.Width * bmp.Height];
            bmp.GetPixels(buffer, 0, bmp.Width, 0, 0, bmp.Width, bmp.Height);
            return RawBitmaps.FromRGB(bmp.Width, bmp.Height, buffer);
            
        }

        public static Bitmap ToAndroidBitmap(this RawBitmap bmp)
            => Bitmap.CreateBitmap(bmp.ToRGB(), bmp.Width, bmp.Height, Bitmap.Config.Argb8888);
    }
}
