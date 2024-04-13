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
                var captureBytes = AppState.CaptureBytes.Get();
                MainThread.InvokeOnMainThreadAsync(() 
                    => Image.Source = ImageSource.FromStream(() => new MemoryStream(captureBytes)));
            });
        }


    }
}