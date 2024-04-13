using LillyScan.Backend.Parallelization;
using LillyScan.FrontendXamarin.Utils;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LillyScan.FrontendXamarin.Camera
{
    public class CameraPreview : View
    {
        public static readonly BindableProperty CameraProperty = BindableProperty.Create(
            propertyName: "Camera",
            returnType: typeof(CameraOptions),
            declaringType: typeof(CameraPreview),
            defaultValue: CameraOptions.Rear);

        public CameraOptions Camera
        {
            get { return (CameraOptions)GetValue(CameraProperty); }
            set { SetValue(CameraProperty, value); }
        }

        public delegate void OnCapturePeeked(object sender, byte[] imageBytes);
        public event OnCapturePeeked CapturePeeked;

        private readonly Atomic<bool> _CapturePeekEnabled = new Atomic<bool>(true);
        public bool CapturePeekEnabled
        {
            get => _CapturePeekEnabled.Get();
            set => _CapturePeekEnabled.Set(value);
        }       

        public void HandleCapture(byte[] imageBytes)
        {
            CapturePeeked?.Invoke(this, imageBytes);            
        }



        public static string Base64Encode(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes);
        }

    }
}
