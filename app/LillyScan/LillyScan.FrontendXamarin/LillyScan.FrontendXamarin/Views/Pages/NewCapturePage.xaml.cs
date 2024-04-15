using LillyScan.Backend.Imaging;
using LillyScan.Backend.Parallelization;
using LillyScan.FrontendXamarin.Models;
using LillyScan.FrontendXamarin.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin.Views.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewCapturePage : ContentPage
    {
        public NewCapturePage()
        {
            InitializeComponent();
        }

        readonly Stopwatch sw = new Stopwatch();

        int k = 0;
        Atomic<CancellationTokenSource> MaskPreviewCancellationTokenSource = new Atomic<CancellationTokenSource>();

        private async void CameraPreview_CapturePeeked(object sender, byte[] imageBytes)
        {
            Console.WriteLine($"[CameraPreview_CapturePeeked] Enter: CanPeek={CanPeek}");

            if(RedirectToProcessing.Get())
            {
                RedirectToProcessing.Set(false);
                AppState.CaptureBytes.Value = imageBytes; // .Set(imageBytes);
                await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//ProcessingPage"));
                return;
            }

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
                MainThread.InvokeOnMainThreadAsync(() => MyButton.Text = $"{sw.ElapsedMilliseconds / 1000.0}s");
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

        ~NewCapturePage()
        {
            Console.WriteLine($"[PreviewPage] Destroyed?: CanPeek={CanPeek} Preview={CameraPreview.CapturePeekEnabled}");
            MaskPreviewCancellationTokenSource.With(SafeCancel);
        }

        Atomic<bool> RedirectToProcessing = false;

        private void CaptureButton_Clicked(object sender, EventArgs e)
        {
            RedirectToProcessing.Set(true);
        }
    }
}