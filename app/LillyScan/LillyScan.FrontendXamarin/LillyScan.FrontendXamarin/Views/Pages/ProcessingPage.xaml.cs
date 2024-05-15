using LillyScan.FrontendXamarin.Utils;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin.Views.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProcessingPage : ContentPage
    {        
        public ProcessingPage()
        {
            InitializeComponent();
            
            Task.Run(() =>
            {
                var captureBytes = AppState.CaptureBytes.Value;
                MainThread.InvokeOnMainThreadAsync(()
                    => Image.Source = ImageSource.FromStream(() => new MemoryStream(captureBytes)));
            });

            AppState.CaptureBytes.ValueChanged += CaptureBytes_ValueChanged;
        }

        private void CaptureBytes_ValueChanged(Backend.Utils.Observable<byte[]> observable, byte[] newValue)
        {
            Task.Run(() =>
            {
                var captureBytes = newValue;
                MainThread.InvokeOnMainThreadAsync(()
                    => Image.Source = ImageSource.FromStream(() => new MemoryStream(captureBytes)));
            });
        }
        

        protected override bool OnBackButtonPressed()
        {
            MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//NewCapturePage"));
            return true;            
        }

        private void LoadFromCaptureBytes(byte[] captureBytes)
        {
            var image = ImageSource.FromStream(() => new MemoryStream(captureBytes));
            MainThread.InvokeOnMainThreadAsync(() => Image.Source = image);

            using(var bitmap = image.ToRawBitmap())
            {
                var normSegmentation = HTR.Engine;


            }

        }

    }
}