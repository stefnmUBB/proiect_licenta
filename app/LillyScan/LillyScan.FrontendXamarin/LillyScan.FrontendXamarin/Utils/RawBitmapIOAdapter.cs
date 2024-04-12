using LillyScan.Backend.Imaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LillyScan.FrontendXamarin.Utils
{
    public static class RawBitmapIOAdapter
    {
        public static Func<ImageSource, Task<RawBitmap>> ImageSource2RawBitmap = _ => throw new NotImplementedException("ImageSource2RawBitmap not set");
        public static Func<RawBitmap, Task<ImageSource>> RawBitmap2ImageSource = _ => throw new NotImplementedException("RawBitmap2ImageSource not set");
        public static async Task<RawBitmap> ToRawBitmap(this ImageSource imageSource) => await ImageSource2RawBitmap(imageSource);
        public static async Task<ImageSource> ToImageSource(this RawBitmap rawBitmap) => await RawBitmap2ImageSource(rawBitmap);
    }
}
