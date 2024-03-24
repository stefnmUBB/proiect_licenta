using Camera.MAUI;
using System.Diagnostics;

namespace LillyScan.Frontend.View;

public partial class CameraPreview : ContentView
{
	public CameraPreview()
	{
		InitializeComponent();
        CameraView.CamerasLoaded += CameraView_CamerasLoaded;        
    }    

    private void CameraView_CamerasLoaded(object? sender, EventArgs e)
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

            Size? resolution = null;
            if (CameraView.Camera.AvailableResolutions.Count > 0)
                resolution = CameraView.Camera.AvailableResolutions.MaxBy(_ => _.Width);

            Debug.WriteLine("Resolutions:");
            Debug.WriteLine(string.Join(", ", CameraView.Camera.AvailableResolutions.Select(_ => _.ToString())));

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await CameraView.StopCameraAsync();

                bool cameraStarted = false;
                if (resolution.HasValue)
                    cameraStarted = await CameraView.StartCameraAsync(resolution.GetValueOrDefault()) == CameraResult.Success;
                else
                    cameraStarted = await CameraView.StartCameraAsync() == CameraResult.Success;

                if (cameraStarted)
                {
                    CameraView.ForceAutoFocus();
                }                
            });
        }
    }
}