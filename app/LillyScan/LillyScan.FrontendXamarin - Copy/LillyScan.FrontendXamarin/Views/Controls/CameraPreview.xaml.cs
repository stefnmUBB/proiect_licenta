using Android.Graphics;
using LillyScan.Backend.Xamarin.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Behaviors;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CameraPreview : ContentView
    {
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

        private TextWriter Log = Console.Out;

        public Task<Bitmap> TakePicture() => Task.Run(() =>
        {
            if (!CameraView.IsAvailable)
            {
                Log?.WriteLine($"[CameraPreview] Not available");
                return null;
            }
            lock (CaptureHolder) 
                CaptureReady = false;
            Log?.WriteLine($"[CameraPreview] Shutter");
            CameraView.Shutter();                        

            lock(CaptureHolder)
            {
                while (!CaptureReady)
                    Monitor.Wait(CaptureHolder);
            }           
            return null as Bitmap;
        });

        private void CameraView_MediaCaptured(object sender, MediaCapturedEventArgs e)
        {
            Log?.WriteLine($"[CameraPreview] MediaCaptured fired");            
            var image = e.Image as StreamImageSource;            
            if(image==null)
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
                using (var ms = new MemoryStream())
                {
                    Log?.WriteLine($"[CameraPreview] Decoding");                    
                    using (var src = await image.Stream(CancellationToken.None))
                        src.CopyTo(ms);
                    var bytes = ms.ToArray();                    
                    var bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
                    Log?.WriteLine($"[CameraPreview] Android.Graphics.Bitmap {bitmap.Width}x{bitmap.Height}");
                    var rawBitmap = RawBitmapIO.FromAndroidBitmap(bitmap);
                    Log?.WriteLine($"[CameraPreview] RawBitmap {rawBitmap.Width}x{rawBitmap.Height}: {rawBitmap[0, 0, 0]} {rawBitmap[0, 0, 1]} {rawBitmap[0, 0, 2]} ...");

                    lock (CaptureHolder)
                    {
                        CaptureReady = true;
                        Monitor.Pulse(CaptureHolder);
                    }
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
    }
}