using LillyScan.Backend.Imaging;
using LillyScan.Backend.Parallelization;
using LillyScan.FrontendXamarin.Camera;
using LillyScan.FrontendXamarin.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;

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
        Atomic<CancellationTokenSource> MaskPreviewCancellationTokenSource = new Atomic<CancellationTokenSource>();

        private async void CameraPreview_CapturePeeked(object sender, byte[] imageBytes)
        {
            Console.WriteLine($"[CameraPreview_CapturePeeked] Enter: CanPeek={CanPeek}");
            if (!CanPeek.Get()) return;            

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

            RawBitmap segmentedBitmap = null;
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                MaskPreviewCancellationTokenSource.Set(cancellationTokenSource);
                var cancellationToken = MaskPreviewCancellationTokenSource.Get().Token;
                try
                {
                    segmentedBitmap = HTR.Engine.SelectTiled64(rawBitmap, parallel: true, preview: true, cancellationToken);
                }
                catch (OperationCanceledException e)
                {
                    Console.WriteLine($"[CameraPreview_CapturePeeked] Operation canceled: {e.Message}");
                    MaskPreviewCancellationTokenSource.Set(null);                    
                }
            }
            sw2.Stop();
            if (segmentedBitmap == null)
            {
                CameraPreview.CapturePeekEnabled = true;
                return;
            }
            Console.WriteLine($"[{sw2.ElapsedMilliseconds}ms] Done segmentedBitmap!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            rawBitmap.Dispose();
            segmentedBitmap = segmentedBitmap.GrayscaleToAlpha(0, 1, 0, 0.5f, disposeOriginal: true);
            imageSource = await segmentedBitmap.ToImageSource();
            segmentedBitmap.Dispose();
            await MainThread.InvokeOnMainThreadAsync(() => Img.Source = imageSource);
            lock (sw) 
            {                
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

        private readonly Atomic<bool> CanPeek = new Atomic<bool>(false);

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CanPeek.Set(true);
            Console.WriteLine($"[PreviewPage] Appeared: CanPeek={CanPeek} Preview={CameraPreview.CapturePeekEnabled}");
        }

        private static void SafeCancel(CancellationTokenSource cts)
        {
            try
            {
                cts?.Cancel();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SafeCancel cts] {e.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            CanPeek.Set(false);
            Console.WriteLine($"[PreviewPage] Disappeared: CanPeek={CanPeek} Preview={CameraPreview.CapturePeekEnabled}");
            MaskPreviewCancellationTokenSource.With(SafeCancel);
        }

        ~AboutPage()
        {
            Console.WriteLine($"[PreviewPage] Destroyed?: CanPeek={CanPeek} Preview={CameraPreview.CapturePeekEnabled}");
            MaskPreviewCancellationTokenSource.With(SafeCancel);
        }
    }
}