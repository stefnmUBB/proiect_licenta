using Android.Graphics;
using Android.Media;
using Android.Opengl;
using Android.Views;
using ImageFromXamarinUI;
using Java.Lang;
using LillyScan.Backend.Imaging;
using LillyScan.FrontendXamarin.BackendAdapt.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CameraPreview : ContentView
    {
        public static Func<Xamarin.Forms.View, int, int, Android.Graphics.Bitmap> CaptureView = null;

        public CameraPreview()
        {
            InitializeComponent();
            
        }

        private void BlitzToggle_CheckedChanged(object sender, EventArgs e)
        {
            Console.WriteLine($"TOGGLE BLITZ: {(BlitzToggle.IsChecked ? CameraFlashMode.On : CameraFlashMode.Off)}");
            CameraView.FlashMode = BlitzToggle.IsChecked ? CameraFlashMode.On : CameraFlashMode.Off;
        }


        private readonly object CaptureHolder = new object();
        private bool CaptureReady = false;

        private readonly TextWriter Log = Console.Out;

        public void Refresh()
        {
            /*CameraView = new CameraView
            {
                CaptureMode = CameraCaptureMode.Photo,
            };
            CameraView.MediaCaptured += CameraView_MediaCaptured;
            CameraView.MediaCaptureFailed += CameraView_MediaCaptureFailed;            */
        }

        public async Task<RawBitmap> TakePicture()
        {
            
            if(CaptureView==null)
            {
                Log?.WriteLine($"[CameraPreview] CaptureView null");
            }
            else
            {
                Log?.WriteLine($"[CameraPreview] CaptureView exists");
                byte[] bytes;
                //var bmp = await MainThread.InvokeOnMainThreadAsync(() => CaptureView(CameraView, 1000, 1000));

                try
                {
                    var ss = await Screenshot.CaptureAsync();

                    //using (var stream = await CameraView.CaptureImageAsync())
                    using (var stream = await ss.OpenReadAsync())
                    {                        
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            bytes = ms.ToArray();
                        }

                    }
                }
                catch(ArgumentException e)
                {
                    Console.WriteLine($"[CameraPreview] {e.Message}");
                    return null;
                }
                catch (IllegalArgumentException e)
                {
                    Console.WriteLine($"[CameraPreview] {e.Message}");
                    return null;
                }

                var bmp = BitmapFactory.DecodeByteArray(bytes,0, bytes.Length);
               
                //var bmp = await MainThread.InvokeOnMainThreadAsync(() => CaptureView(CameraView, 1000, 1000));
                if (bmp == null) return null;
                Log?.WriteLine($"[CameraPreview] Bitmap {bmp?.Width ?? -1} {bmp?.Height ?? -1}");
                Log?.WriteLine($"[CameraPreview] Android.Graphics.Bitmap {bmp.Width}x{bmp.Height}");

                var rawBitmap = RawBitmapIO.FromAndroidBitmap(bmp);
                Log?.WriteLine($"[CameraPreview] RawBitmap {rawBitmap.Width}x{rawBitmap.Height}: {rawBitmap[0, 0, 0]} {rawBitmap[0, 0, 1]} {rawBitmap[0, 0, 2]} ...");                
                rawBitmap.Dispose();                
                
                using (var ms = new MemoryStream())
                {
                    bmp.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, 0, ms);
                    bytes = ms.ToArray();
                }
                bmp.Dispose();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var img = ImageSource.FromStream(() => new MemoryStream(bytes));
                    Img.Source = img;
                });
                               
                
            }

            return null;
            if (!CameraView.IsAvailable)
            {
                Log?.WriteLine($"[CameraPreview] Not available");
                return null;
            }
            lock (CaptureHolder)
                CaptureReady = false;
            Log?.WriteLine($"[CameraPreview] Shutter {CameraView.ShutterCommand?.ToString() ?? "null"}");            
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() => CameraView.Shutter());                
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"[CameraPreview] {e.GetType()}:{e.Message}");
                return null;
            }

            lock (CaptureHolder)
            {
                while (!CaptureReady)
                    Monitor.Wait(CaptureHolder);
            }
            return null;
        }

        private void CameraView_MediaCaptured(object sender, MediaCapturedEventArgs e)
        {
            Log?.WriteLine($"[CameraPreview] MediaCaptured fired");
            var imageData = e.ImageData;            
            //var image = e.Image as StreamImageSource;
            if (imageData == null)
            {
                Log?.WriteLine($"[CameraPreview] ImageSource is null");
                lock (CaptureHolder)
                {
                    CaptureReady = true;
                    Monitor.Pulse(CaptureHolder);
                }                
            }            

            Task.Run(async () =>
            {                
                Log?.WriteLine($"[CameraPreview] PPH");
                System.Threading.Thread.Sleep(2000);
                byte[] bytes = null;

                using (var ms = new MemoryStream(imageData))
                {
                    Log?.WriteLine($"[CameraPreview] Decoding");
                    //using (var src = await image.Stream(CancellationToken.None))
                        //src.CopyTo(ms);
                    bytes = ms.ToArray();
                }
                using (var bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length))
                {
                    Log?.WriteLine($"[CameraPreview] Android.Graphics.Bitmap {bitmap.Width}x{bitmap.Height}");
                    var rawBitmap = RawBitmapIO.FromAndroidBitmap(bitmap);
                    Log?.WriteLine($"[CameraPreview] RawBitmap {rawBitmap.Width}x{rawBitmap.Height}: {rawBitmap[0, 0, 0]} {rawBitmap[0, 0, 1]} {rawBitmap[0, 0, 2]} ...");
                    rawBitmap.Dispose();
                }

                lock (CaptureHolder)
                {
                    CaptureReady = true;
                    Monitor.Pulse(CaptureHolder);
                }                
            });
        }

        private void CameraView_MediaCaptureFailed(object sender, string e)
        {
            Log?.WriteLine($"[CameraPreview] MediaCapturedFailed fired: {e}");
            lock (CaptureHolder)
            {
                CaptureReady = true;
                Monitor.Pulse(CaptureHolder);
            }
        }

        
        private void CaptureScreenshot(Xamarin.Forms.View view)
        {            



        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Log?.WriteLine($"[CameraPreview] Clicked!");
            CameraView.Shutter();
            
            //PixelCopy.Request()
        }


        /*public static readonly BindableProperty OverlayViewProperty =
            BindableProperty.Create(nameof(OverlayView), typeof(View), typeof(CameraPreview), propertyChanged: OnOverlayViewChanged);

        public View OverlayView { get => (View)GetValue(OverlayViewProperty); set => SetValue(OverlayViewProperty, value); }


        static void OnOverlayViewChanged(object bindable, object oldValue, object newValue)
        {
            var page = bindable as CameraPreview;
            page.OverlayContent.Children.Clear();
            if (newValue is View view)
                page.OverlayContent.Children.Add(view);            
        }*/

    }
}