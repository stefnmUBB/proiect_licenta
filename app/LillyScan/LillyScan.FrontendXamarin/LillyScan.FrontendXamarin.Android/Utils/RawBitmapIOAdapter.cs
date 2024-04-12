﻿using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LillyScan.Backend.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace LillyScan.FrontendXamarin.Droid.Utils
{
    public static class RawBitmapIOAdapter
    {
        public static async Task<RawBitmap> ToRawBitmap(ImageSource imageSource, bool includeAlpha=false)
        {            
            if (imageSource is StreamImageSource sis)
            {
                using var stream = await sis.Stream(CancellationToken.None);
                var bitmap = BitmapFactory.DecodeStream(stream);
                var buffer = new int[bitmap.Width * bitmap.Height];
                bitmap.GetPixels(buffer, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);
                int width = bitmap.Width;
                int height = bitmap.Height;
                bitmap.Recycle();                
                var bmp =  RawBitmaps.FromRGB(width, height, buffer, includeAlpha);                
                return bmp;
            }            
            else
            {
                throw new NotImplementedException();
            }            
        }


        public static async Task<ImageSource> ToImageSource(RawBitmap rawBitmap)
        {                        
            int[] colors = rawBitmap.ToRGB();            
            var nativeBitmap = Bitmap.CreateBitmap(colors, rawBitmap.Width, rawBitmap.Height, Bitmap.Config.Argb8888);            
            using var ms = new MemoryStream();
            await nativeBitmap.CompressAsync(rawBitmap.Channels == 4 ? Bitmap.CompressFormat.Png : Bitmap.CompressFormat.Jpeg, 90, ms);
            nativeBitmap.Recycle();
            var bytes = ms.ToArray();
            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }
    }
}