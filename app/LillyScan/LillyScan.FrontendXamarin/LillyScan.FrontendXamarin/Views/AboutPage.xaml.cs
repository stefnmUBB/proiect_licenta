using LillyScan.Backend.Imaging;
using LillyScan.Backend.Utils;
using LillyScan.FrontendXamarin.Utils;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();            
        }

        readonly Stopwatch sw = new Stopwatch();

        int k = 0;

        private async void CameraPreview_CapturePeeked(object sender, byte[] imageBytes)
        {
            Console.WriteLine("CameraPreview_CapturePeeked");
            Console.WriteLine($"Frame {k++}");
            lock (sw)
            {
                if (sw.IsRunning) 
                    return;
                sw.Restart();
            }

            var imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            var rawBitmap = await imageSource.ToRawBitmap();
            //imageSource = await rawBitmap.ToImageSource();

            using (var segmentedBitmap = HTR.Engine.SelectTiled64(rawBitmap))
            {
                Console.WriteLine("Done segmentedBitmap!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                rawBitmap.Dispose();
                imageSource = await segmentedBitmap.ToImageSource();                
            }
            /*using (var segmentedBitmap = HTR.Engine.SelectTiled64(rawBitmap))
            {
                rawBitmap.Dispose();
                imageSource = await segmentedBitmap.ToImageSource();
            }*/

            lock (sw) 
            {
                /*if (sw.IsRunning && sw.ElapsedMilliseconds < 100)
                    return;                
                sw.Restart();                */
                
                MainThread.InvokeOnMainThreadAsync(() =>
                {                    
                    Img.Source = imageSource;
                });
                sw.Stop();
            }

        }

        private void Canvas_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(new SkiaSharp.SKColor(0, 0, 255, 64));
        }
    }
}