using LillyScan.Backend.Imaging;
using LillyScan.Backend.Utils;
using LillyScan.FrontendXamarin.Camera;
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
            CameraPreview.CapturePeekEnabled = false;
            Console.WriteLine($"CameraPreview_CapturePeeked {DateTime.Now}");
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

            var sw2 = new Stopwatch();
            sw2.Start();
            var segmentedBitmap = HTR.Engine.SelectTiled64(rawBitmap, parallel: true);            
            sw2.Stop();                
            Console.WriteLine($"[{sw2.ElapsedMilliseconds}ms] Done segmentedBitmap!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            rawBitmap.Dispose();
            segmentedBitmap = segmentedBitmap.GrayscaleToAlpha(0, 1, 0, 0.5f, disposeOriginal: true);
            imageSource = await segmentedBitmap.ToImageSource();
            segmentedBitmap.Dispose();

            lock (sw) 
            {                                
                MainThread.InvokeOnMainThreadAsync(() =>
                {                    
                    Img.Source = imageSource;
                });
                sw.Stop();
                Console.WriteLine($"Processed frame in {sw.ElapsedMilliseconds / 1000.0}s");
            }
            CameraPreview.CapturePeekEnabled = true;
        }

        private void Canvas_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(new SkiaSharp.SKColor(0, 0, 255, 64));
        }
    }
}