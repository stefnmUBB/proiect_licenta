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

        public void HandleCapture(byte[] imageBytes)
        {
            CapturePeeked?.Invoke(this, imageBytes);
            //var imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            //Console.WriteLine(bytes.Length);
            /*var str = Base64Encode(bytes);

            for (int i = 0; i < System.Math.Min(str.Length, 100); i++)
            {
                Console.Write(str[i]);
                if (i % 3000 == 0) Console.WriteLine();
            }

            //Console.WriteLine(str);*/
            //Console.WriteLine(str.Length);
            //MainThread.InvokeOnMainThreadAsync(()=>Img.)
        }



        public static string Base64Encode(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes);
        }

    }
}
