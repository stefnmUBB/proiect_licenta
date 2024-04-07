using Camera.MAUI;
using LillyScan.Backend.Imaging;
using LillyScan.Backend.MAUI.Imaging;
using Microsoft.Maui.Graphics.Platform;
using System.Diagnostics;

namespace LillyScan.Frontend.View;

public partial class CameraPreview : ContentView
{
	public CameraPreview()
	{
		InitializeComponent();
        CameraView.CamerasLoaded += CameraView_CamerasLoaded;        
        
    }

    public bool AreCamerasLoaded { get; private set; } = false;

    private void StartCamera(Action callback = null)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Size? resolution = null;
            if (CameraView.Camera.AvailableResolutions.Count > 0)
                resolution = CameraView.Camera.AvailableResolutions.MaxBy(_ => _.Width);

            Debug.WriteLine("Resolutions:");
            Debug.WriteLine(string.Join(", ", CameraView.Camera.AvailableResolutions.Select(_ => _.ToString())));

            await CameraView.StopCameraAsync();

            bool cameraStarted = false;
            if (resolution.HasValue)
                cameraStarted = await CameraView.StartCameraAsync(resolution.GetValueOrDefault()) == CameraResult.Success;
            else
                cameraStarted = await CameraView.StartCameraAsync() == CameraResult.Success;

            if (cameraStarted)
            {
                CameraView.ForceAutoFocus();
                callback?.Invoke();
            }
        });
    }

    private void CameraView_CamerasLoaded(object sender, EventArgs e)
    {
        Debug.WriteLine("Logging works???");
        Debug.WriteLine($"Cameras detected: {CameraView.NumCamerasDetected}");

        if (CameraView.NumCamerasDetected == 0)
            Thread.Sleep(500);

        if (CameraView.NumCamerasDetected > 0)
        {
            if (CameraView.NumMicrophonesDetected > 0)
                CameraView.Microphone = CameraView.Microphones.First();

            CameraView.Camera = CameraView.Cameras.First();            

            StartCamera();
            lock (this)
                AreCamerasLoaded = true;
        }        
    }

    public ImageSource GetSnapshot() => CameraView.GetSnapShot();

    public async Task<RawBitmap> GetSnapshotRaw()
    {
        lock (this)
        {
            if (!AreCamerasLoaded)
                return null;
        }
        //CameraView.TakePhotoAsync(Camera.MAUI.ImageFormat.PNG)
        var pic = CameraView.GetSnapShot() as StreamImageSource;
        if (pic == null) return null;
        using var stream = await pic.Stream(CancellationToken.None);
        using var image = PlatformImage.FromStream(stream);
        return RawBitmapIO.FromBitmap(image);
    }

    private void BlitzCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() => CameraView.TorchEnabled = BlitzCheckBox.IsChecked);
        }
#if ANDROID
        catch(Java.Lang.IllegalStateException ex)
        {
            Console.WriteLine($"{ex.GetType()}: {ex.Message}");
            Console.WriteLine($"Restarting camera");
            StartCamera(callback: () => CameraView.TorchEnabled = BlitzCheckBox.IsChecked);
        }
#endif
        finally { }
    }
}