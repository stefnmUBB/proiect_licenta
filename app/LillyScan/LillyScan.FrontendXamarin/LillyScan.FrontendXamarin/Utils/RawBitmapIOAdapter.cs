using LillyScan.Backend.Imaging;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LillyScan.FrontendXamarin.Utils
{
    public static class RawBitmapIOAdapter
    {
        public static Func<ImageSource, bool, Task<RawBitmap>> ImageSource2RawBitmap = (_, a) => throw new NotImplementedException("ImageSource2RawBitmap not set");
        public static Func<RawBitmap, Task<ImageSource>> RawBitmap2ImageSource = _ => throw new NotImplementedException("RawBitmap2ImageSource not set");
        public static async Task<RawBitmap> ToRawBitmap(this ImageSource imageSource, bool includeAlpha = false) 
            => await ImageSource2RawBitmap(imageSource, includeAlpha);

        public static RawBitmap ToRawBitmapSync(this ImageSource imageSource, bool includeAlpha = false)
        {
            var task = Task.Run(async () => await ToRawBitmap(imageSource, includeAlpha));
            task.Wait();
            return task.Result;
        }

        public static async Task<ImageSource> ToImageSource(this RawBitmap rawBitmap) 
            => await RawBitmap2ImageSource(rawBitmap);
    }
}
