using LillyScan.Backend.Imaging;
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
            
            Task.Run(async () =>
            {
                var captureBytes = AppState.CaptureBytes.Value;
                await LoadFromCaptureBytes(captureBytes);                
            });

            AppState.CaptureBytes.ValueChanged += CaptureBytes_ValueChanged;

            ProcessingState = ProcessingState.Pending;

            InputRetryButton.Clicked += InputRetryButton_Clicked;
            InputConfirmButton.Clicked += InputConfirmButton_Clicked;
        }

        private void InputConfirmButton_Clicked(object sender, System.EventArgs e)
        {
            ProcessingState = ProcessingState.Running;
        }

        private void InputRetryButton_Clicked(object sender, System.EventArgs e)
        {
            MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//NewCapturePage"));
        }

        public ProcessingState ProcessingState
        {
            get => (ProcessingState)GetValue(ProcessingStateProperty);
            set => SetValue(ProcessingStateProperty, value);            
        }

        public static readonly BindableProperty ProcessingStateProperty = BindableProperty
            .Create(nameof(ProcessingState), typeof(ProcessingState), typeof(ProcessingPage), ProcessingState.Pending);

        private void CaptureBytes_ValueChanged(Backend.Utils.Observable<byte[]> observable, byte[] newValue)
        {
            Task.Run(async () =>
            {
                var captureBytes = newValue;
                await LoadFromCaptureBytes(captureBytes);                
            });
        }        

        protected override bool OnBackButtonPressed()
        {
            MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//NewCapturePage"));
            return true;            
        }

        private async Task<int> LoadFromCaptureBytes(byte[] captureBytes)
        {
            var image = ImageSource.FromStream(() => new MemoryStream(captureBytes));
            await MainThread.InvokeOnMainThreadAsync(() => Image.Source = image);
            using (var bitmap = await image.ToRawBitmap()) 
            {
                var segmentation = HTR.Engine.SegmentTiles64(bitmap, Backend.HTR.SegmentationType.PaddedLinear);
                segmentation = segmentation.GrayscaleToAlpha(0, 1, 0, 0.5f, disposeOriginal: true);
                var segImageSource = await segmentation.ToImageSource();
                await MainThread.InvokeOnMainThreadAsync(() => MaskImage.Source = segImageSource);
            }
            return 0;
        }

    }
}