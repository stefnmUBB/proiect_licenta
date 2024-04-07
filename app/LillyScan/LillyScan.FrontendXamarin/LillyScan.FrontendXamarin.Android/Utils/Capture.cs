using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace LillyScan.FrontendXamarin.Droid.Utils
{
    internal static class Capture
    {
        public static Android.Graphics.Bitmap CaptureView(Xamarin.Forms.View v, int desiredWidth, int desiredHeight)
        {                               
            var rend = Platform.GetRenderer(v);
            Console.WriteLine($"rend = {rend}");
            if(rend==null)
            {
                Console.WriteLine($"[CaptureView] rend is null");
                return null;
            }            
            rend.Tracker.UpdateLayout();
            Android.Views.View view = rend.View;
            Console.WriteLine($"view = {view}");

            if (view == null)
                throw new NullReferenceException("Unable to find the main window.");

            /*Console.WriteLine($"[CaptureView] Measure");
            view.Measure(desiredWidth, desiredHeight);
            Console.WriteLine($"[CaptureView] Layout");
            view.Layout(0, 0, desiredWidth, desiredHeight);*/

            Console.WriteLine($"[CaptureView] Creating bitmap {view.Width} {view.Height}");

            if(view.Width<=0 || view.Height<=0)
            {
                Console.WriteLine($"[CaptureView] Invalid size");
                return null;
            }

            var bitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888);

            Console.WriteLine($"[CaptureView] bitmap =  {bitmap}");

            Console.WriteLine($"[CaptureView] Creating canvas");
            using (var canvas = new Canvas(bitmap))
            {
                var drawable = view.Background;
                if (drawable != null)
                    drawable.Draw(canvas);
                else
                    canvas.DrawColor(Android.Graphics.Color.Red);
                view.SetLayerType(LayerType.None, null);
                view.Draw(canvas);
            }

            return bitmap;            
        }
    }
}