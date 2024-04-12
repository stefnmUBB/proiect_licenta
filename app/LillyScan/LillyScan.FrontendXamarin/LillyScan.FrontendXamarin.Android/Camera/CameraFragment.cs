using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using Java.Lang;
using Java.Util.Concurrent;
using LillyScan.FrontendXamarin.Camera;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
namespace LillyScan.FrontendXamarin.Droid.Camera
{
    class CameraFragment : Fragment, TextureView.ISurfaceTextureListener
    {
        CameraDevice device;
        CaptureRequest.Builder sessionBuilder;
        CameraCaptureSession session;
        CameraTemplate cameraTemplate;
        CameraManager manager;

        bool cameraPermissionsGranted;
        bool busy;
        bool repeatingIsRunning;
        int sensorOrientation;
        string cameraId;
        LensFacing cameraType;

        Android.Util.Size previewSize;

        HandlerThread backgroundThread;
        Handler backgroundHandler = null;

        Java.Util.Concurrent.Semaphore captureSessionOpenCloseLock = new Java.Util.Concurrent.Semaphore(1);

        AutoFitTextureView texture;

        TaskCompletionSource<CameraDevice> initTaskSource;
        TaskCompletionSource<bool> permissionsRequested;

        CameraManager Manager => manager ??= (CameraManager)Context.GetSystemService(Context.CameraService);

        bool IsBusy
        {
            get => device == null || busy;
            set
            {
                busy = value;
            }
        }

        bool Available;

        public CameraPreview Element { get; set; }

        #region Constructors

        public CameraFragment()
        {
            CaptureCallback.CapturePeeked += CaptureCallback_CapturePeeked;
        }        

        public CameraFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            CaptureCallback.CapturePeeked += CaptureCallback_CapturePeeked;
        }
        #endregion

        #region Overrides

        public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) => inflater.Inflate(Resource.Layout.CameraFragment, null);
        public override void OnViewCreated(Android.Views.View view, Bundle savedInstanceState) => texture = view.FindViewById<AutoFitTextureView>(Resource.Id.cameratexture);

        public override void OnPause()
        {
            CloseSession();
            StopBackgroundThread();
            base.OnPause();
        }

        public override async void OnResume()
        {
            base.OnResume();

            StartBackgroundThread();
            if (texture is null)
            {
                return;
            }
            if (texture.IsAvailable)
            {
                View?.SetBackgroundColor(Element.BackgroundColor.ToAndroid());
                cameraTemplate = CameraTemplate.Preview;
                await RetrieveCameraDevice(force: true);
            }
            else
            {
                texture.SurfaceTextureListener = this;
            }
        }

        protected override void Dispose(bool disposing)
        {
            CloseDevice();
            base.Dispose(disposing);
        }

        #endregion

        #region Public methods

        public async Task RetrieveCameraDevice(bool force = false)
        {
            if (Context == null || (!force && initTaskSource != null))
            {
                return;
            }

            if (device != null)
            {
                CloseDevice();
            }

            await RequestCameraPermissions();
            if (!cameraPermissionsGranted)
            {
                return;
            }

            if (!captureSessionOpenCloseLock.TryAcquire(2500, TimeUnit.Milliseconds))
            {
                throw new RuntimeException("Timeout waiting to lock camera opening.");
            }

            IsBusy = true;
            cameraId = GetCameraId();

            if (string.IsNullOrEmpty(cameraId))
            {
                IsBusy = false;
                captureSessionOpenCloseLock.Release();
                Console.WriteLine("No camera found");
            }
            else
            {
                try
                {
                    CameraCharacteristics characteristics = Manager.GetCameraCharacteristics(cameraId);
                    StreamConfigurationMap map = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);

                    previewSize = ChooseOptimalSize(map.GetOutputSizes(Class.FromType(typeof(SurfaceTexture))),
                        texture.Width, texture.Height, GetMaxSize(map.GetOutputSizes((int)ImageFormatType.Jpeg)));
                    sensorOrientation = (int)characteristics.Get(CameraCharacteristics.SensorOrientation);
                    cameraType = (LensFacing)(int)characteristics.Get(CameraCharacteristics.LensFacing);

                    if (Resources.Configuration.Orientation == Android.Content.Res.Orientation.Landscape)
                    {
                        //texture.SetAspectRatio(previewSize.Width, previewSize.Height);                        
                    }
                    else
                    {
                        //texture.SetAspectRatio(previewSize.Height, previewSize.Width);
                    }                    

                    initTaskSource = new TaskCompletionSource<CameraDevice>();
                    Manager.OpenCamera(cameraId, new CameraStateListener
                    {
                        OnOpenedAction = device => initTaskSource?.TrySetResult(device),
                        OnDisconnectedAction = device =>
                        {
                            initTaskSource?.TrySetResult(null);
                            CloseDevice(device);
                        },
                        OnErrorAction = (device, error) =>
                        {
                            initTaskSource?.TrySetResult(device);
                            Console.WriteLine($"Camera device error: {error}");
                            CloseDevice(device);
                        },
                        OnClosedAction = device =>
                        {
                            initTaskSource?.TrySetResult(null);
                            CloseDevice(device);
                        }
                    }, backgroundHandler);

                    captureSessionOpenCloseLock.Release();
                    device = await initTaskSource.Task;
                    initTaskSource = null;
                    if (device != null)
                    {
                        await PrepareSession();
                    }
                }
                catch (Java.Lang.Exception ex)
                {
                    Console.WriteLine("Failed to open camera. ", ex);
                    Available = false;
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        public void UpdateRepeatingRequest()
        {
            if (session == null || sessionBuilder == null)
            {
                return;
            }

            IsBusy = true;
            try
            {
                if (repeatingIsRunning)
                {
                    session.StopRepeating();
                }

                sessionBuilder.Set(CaptureRequest.ControlMode, (int)ControlMode.Auto);
                sessionBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.On);
                session.SetRepeatingRequest(sessionBuilder.Build(), listener: CaptureCallback, backgroundHandler);
                repeatingIsRunning = true;
            }
            catch (Java.Lang.Exception error)
            {
                Console.WriteLine("Update preview exception.", error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        readonly PictureCaptureCallback CaptureCallback = new PictureCaptureCallback();

        //public delegate void OnImage

        class PictureCaptureCallback : CameraCaptureSession.CaptureCallback
        {
            public Android.Util.Size Size { get; set; }
            public Surface Surface { get; set; }
            public Handler Handler { get; set; }
            public CameraFragment Fragment { get; set; }

            public delegate void OnCapturePeeked(byte[] imageBytes);
            public event OnCapturePeeked CapturePeeked;

            public override void OnCaptureCompleted(CameraCaptureSession session, CaptureRequest request, TotalCaptureResult result)
            {
                base.OnCaptureCompleted(session, request, result);
                Console.WriteLine("OnCaptureCompleted");
                if (Surface == null) return;

                Console.WriteLine("Surface");
                var listener = new PixelCopyFinishedListener();
                Console.WriteLine($"{Size.Width} {Size.Height}");
                using (var bmp = Bitmap.CreateBitmap(Size.Width, Size.Height, Bitmap.Config.Argb8888))
                {
                    Console.WriteLine("Request");
                    PixelCopy.Request(Surface, bmp, listener, Handler);
                    Console.WriteLine("Request done");

                    byte[] bytes = null;
                    using (var ms = new MemoryStream())
                    {
                        bmp.Compress(Bitmap.CompressFormat.Jpeg, 90, ms);
                        bytes = ms.ToArray();
                    }
                    bmp.Recycle();
                    CapturePeeked?.Invoke(bytes);
                }
            }
            class PixelCopyFinishedListener : Java.Lang.Object, PixelCopy.IOnPixelCopyFinishedListener
            {
                public void OnPixelCopyFinished(int copyResult)
                {
                    Console.WriteLine($"Pixel copy finished: {copyResult}");
                }
            }

        }
        #endregion

        void StartBackgroundThread()
        {
            backgroundThread = new HandlerThread("CameraBackground");
            backgroundThread.Start();
            backgroundHandler = new Handler(backgroundThread.Looper);
        }

        void StopBackgroundThread()
        {
            if (backgroundThread == null)
            {
                return;
            }

            backgroundThread.QuitSafely();
            try
            {
                backgroundThread.Join();
                backgroundThread = null;
                backgroundHandler = null;
            }
            catch (InterruptedException ex)
            {
                Console.WriteLine("Error stopping background thread.", ex);
            }
        }

        public delegate void OnCapturePeeked(byte[] imageBytes);
        public event OnCapturePeeked CapturePeeked;

        private void CaptureCallback_CapturePeeked(byte[] imageBytes)
        {
            Console.WriteLine("Here?");
            CapturePeeked?.Invoke(imageBytes);
        }

        Android.Util.Size GetMaxSize(Android.Util.Size[] imageSizes)
        {
            Android.Util.Size maxSize = null;
            long maxPixels = 0;
            for (int i = 0; i < imageSizes.Length; i++)
            {
                long currentPixels = imageSizes[i].Width * imageSizes[i].Height;
                if (currentPixels > maxPixels)
                {
                    maxSize = imageSizes[i];
                    maxPixels = currentPixels;
                }
            }
            return maxSize;
        }

        Android.Util.Size ChooseOptimalSize(Android.Util.Size[] choices, int width, int height, Android.Util.Size aspectRatio)
        {
            List<Android.Util.Size> bigEnough = new List<Android.Util.Size>();
            int w = aspectRatio.Width;
            int h = aspectRatio.Height;

            foreach (Android.Util.Size option in choices)
            {
                if (option.Height == option.Width * h / w && option.Width >= width && option.Height >= height)
                {
                    bigEnough.Add(option);
                }
            }
            Console.WriteLine(string.Join(", ", choices.Select(_ => $"({_.Width}, {_.Height})")));

            if (bigEnough.Count > 0)
            {
                //var sorted = bigEnough.OrderBy(s => s.Width * s.Height).ToArray();
                //return sorted[sorted.Length / 2];
                return bigEnough.OrderBy(s => s.Width * s.Height).First();
                //int minArea = bigEnough.Min(s => s.Width * s.Height);
                //return bigEnough.First(s => s.Width * s.Height == minArea);
            }
            else
            {
                Console.WriteLine("Couldn't find any suitable preview size.");
                return choices[0];
            }
        }

        string GetCameraId()
        {
            string[] cameraIdList = Manager.GetCameraIdList();
            if (cameraIdList.Length == 0)
            {
                return null;
            }

            string FilterCameraByLens(LensFacing lensFacing)
            {
                foreach (string id in cameraIdList)
                {
                    CameraCharacteristics characteristics = Manager.GetCameraCharacteristics(id);
                    if (lensFacing == (LensFacing)(int)characteristics.Get(CameraCharacteristics.LensFacing))
                    {
                        return id;
                    }
                }
                return null;
            }

            return (Element.Camera == CameraOptions.Front) ? FilterCameraByLens(LensFacing.Front) : FilterCameraByLens(LensFacing.Back);
        }

        async Task PrepareSession()
        {
            IsBusy = true;
            try
            {
                CloseSession();
                sessionBuilder = device.CreateCaptureRequest(cameraTemplate);

                List<Surface> surfaces = new List<Surface>();
                if (texture.IsAvailable && previewSize != null)
                {
                    var texture = this.texture.SurfaceTexture;
                    texture.SetDefaultBufferSize(previewSize.Width, previewSize.Height);
                    Surface previewSurface = new Surface(texture);
                    surfaces.Add(previewSurface);
                    sessionBuilder.AddTarget(previewSurface);
                    CaptureCallback.Surface = previewSurface;
                    CaptureCallback.Size = previewSize;
                    CaptureCallback.Handler = backgroundHandler;
                    CaptureCallback.Fragment = this;                    
                }

                TaskCompletionSource<CameraCaptureSession> tcs = new TaskCompletionSource<CameraCaptureSession>();
                device.CreateCaptureSession(surfaces, new CameraCaptureStateListener
                {
                    OnConfigureFailedAction = captureSession =>
                    {
                        tcs.SetResult(null);
                        Console.WriteLine("Failed to create capture session.");
                    },
                    OnConfiguredAction = captureSession => tcs.SetResult(captureSession)
                }, null);

                session = await tcs.Task;
                if (session != null)
                {
                    UpdateRepeatingRequest();
                }
            }
            catch (Java.Lang.Exception ex)
            {
                Available = false;
                Console.WriteLine("Capture error.", ex);
            }
            finally
            {
                Available = session != null;
                IsBusy = false;
            }
        }

        void CloseSession()
        {
            repeatingIsRunning = false;
            if (session == null)
            {
                return;
            }

            try
            {
                session.StopRepeating();
                session.AbortCaptures();
                session.Close();
                session.Dispose();
                session = null;
            }
            catch (CameraAccessException ex)
            {
                Console.WriteLine("Camera access error.", ex);
            }
            catch (Java.Lang.Exception ex)
            {
                Console.WriteLine("Error closing device.", ex);
            }
        }

        void CloseDevice(CameraDevice inputDevice)
        {
            if (inputDevice == device)
            {
                CloseDevice();
            }
        }

        void CloseDevice()
        {
            CloseSession();

            try
            {
                if (sessionBuilder != null)
                {
                    sessionBuilder.Dispose();
                    sessionBuilder = null;
                }
                if (device != null)
                {
                    device.Close();
                    device = null;
                }
            }
            catch (Java.Lang.Exception error)
            {
                Console.WriteLine("Error closing device.", error);
            }
        }

        void ConfigureTransform(int viewWidth, int viewHeight)
        {
            if (texture == null || previewSize == null || previewSize.Width == 0 || previewSize.Height == 0)
                return;

            Console.WriteLine($"CfgTransform");
            Console.WriteLine($"View: {viewWidth}, {viewHeight}");
            Console.WriteLine($"Prev: {previewSize.Width}, {previewSize.Height}");
            Console.WriteLine($"Elem: {Element.Width}, {Element.Height}");

            var matrix = new Matrix();
            //matrix.SetScale((float)(Element.Width / previewSize.Width), 1f);
            /*var viewRect = new RectF(0, 0, viewWidth, viewHeight);
            var bufferRect = new RectF(0, 0, previewSize.Width, previewSize.Height);
            var centerX = viewRect.CenterX();
            var centerY = viewRect.CenterY();
            bufferRect.Offset(centerX + bufferRect.CenterX(), centerY + bufferRect.CenterY());            
            matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);            
            matrix.PostRotate(GetCaptureOrientation(), centerX, centerY);*/
            texture.SetTransform(matrix);
        }
        
        int GetCaptureOrientation()
        {
            int frontOffset = cameraType == LensFacing.Front ? 90 : -90;
            return (360 + sensorOrientation - GetDisplayRotationDegrees() + frontOffset) % 360;
        }

        int GetDisplayRotationDegrees() =>
            GetDisplayRotation() switch
            {
                SurfaceOrientation.Rotation90 => 90,
                SurfaceOrientation.Rotation180 => 180,
                SurfaceOrientation.Rotation270 => 270,
                _ => 0
            };

        SurfaceOrientation GetDisplayRotation() => Android.App.Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>().DefaultDisplay.Rotation;

        #region Permissions

        async Task RequestCameraPermissions()
        {
            if (permissionsRequested != null)
            {
                await permissionsRequested.Task;
            }

            List<string> permissionsToRequest = new List<string>();
            cameraPermissionsGranted = ContextCompat.CheckSelfPermission(Context, Manifest.Permission.Camera) == Permission.Granted;
            if (!cameraPermissionsGranted)
            {
                permissionsToRequest.Add(Manifest.Permission.Camera);
            }

            if (permissionsToRequest.Count > 0)
            {
                permissionsRequested = new TaskCompletionSource<bool>();
                RequestPermissions(permissionsToRequest.ToArray(), requestCode: 1);
                await permissionsRequested.Task;
                permissionsRequested = null;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode != 1)
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                return;
            }

            for (int i = 0; i < permissions.Length; i++)
            {
                if (permissions[i] == Manifest.Permission.Camera)
                {
                    cameraPermissionsGranted = grantResults[i] == Permission.Granted;
                    if (!cameraPermissionsGranted)
                    {
                        Console.WriteLine("No permission to use the camera.");
                    }
                }
            }
            permissionsRequested?.TrySetResult(true);
        }

        #endregion

        #region TextureView.ISurfaceTextureListener

        async void TextureView.ISurfaceTextureListener.OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            View?.SetBackgroundColor(Element.BackgroundColor.ToAndroid());
            cameraTemplate = CameraTemplate.Preview;
            await RetrieveCameraDevice();
        }

        bool TextureView.ISurfaceTextureListener.OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            CloseDevice();
            return true;
        }

        void TextureView.ISurfaceTextureListener.OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height) => ConfigureTransform(width, height);

        void TextureView.ISurfaceTextureListener.OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
            View.SetBackgroundColor(Android.Graphics.Color.Blue);            
        }

        #endregion
    }
}