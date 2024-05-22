using LillyScan.Backend.Imaging;
using LillyScan.Backend.Parallelization;
using LillyScan.Backend.Utils;
using LillyScan.FrontendXamarin.Models;
using LillyScan.FrontendXamarin.Utils;
using LillyScan.FrontendXamarin.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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

        private void SetVerticalViewSplit(double value)
        {
            AbsoluteLayout.SetLayoutBounds(PreviewMaskGrid, new Rectangle(0, 0, 1, value));
            AbsoluteLayout.SetLayoutBounds(PreviewLinePredictionList, new Rectangle(0, 1, 1, 1 - value));
        }

        private void InputConfirmButton_Clicked(object sender, System.EventArgs e)
        {
            ProcessingState = ProcessingState.Running;                       
            Task.Run(ProcessInput);
        }

        private async void ProcessInput()
        {
            var image = ImageSource.FromStream(() => new MemoryStream(AppState.CaptureBytes.Value));
            var pm = new ProgressMonitor();
            pm.ProgressChanged += (o, progress, description) =>
            {
                MainThread.InvokeOnMainThreadAsync(() => ProgressBar.Percentage = progress);
                //Debug.WriteLine($"[{progress:000.00}] {description}");
            };

            using (var bitmap = await image.ToRawBitmap())
            {
                var lines = HTR.Engine.SegmentLines(bitmap, pm);
                foreach (var mask in lines)
                {
                    var linebmp = mask.CutFromImage(bitmap);
                    linebmp.CheckNaN();
                    linebmp = linebmp.RotateAndCrop((float)-System.Math.Atan2(-mask.LineFit.A, mask.LineFit.B), disposeOriginal: true);
                    linebmp.CheckNaN();
                    var linePred = new PreviewLinePrediction { LineImage = await linebmp.ToImageSource() };
                    await MainThread.InvokeOnMainThreadAsync(() => PreviewLinePredictionList.AddItem(linePred));
                    linebmp.Dispose();                    
                }
            }
            new Animation((t) => SetVerticalViewSplit(t), 1, 0.3).Commit(this, "SlideViewsAnim", 16, 500);

            RunPredictions();
        }        

        private void SetProgressBarPercentage(double value)
        {
            MainThread.InvokeOnMainThreadAsync(() => ProgressBar.Percentage = value);
        }


        private void RunPredictions()
        {            
            Atomic<int> finished = 0;
            int count = PreviewLinePredictionList.ItemsCount;
            SetProgressBarPercentage(count == 0 ? 1 : 0);

            PreviewLinePredictionList.ForeachItem(linePred =>
            {
                using var linebmp = linePred.LineImage.ToRawBitmapSync();
                //linePred.PredictedText = HTR.Engine.PredictTextLine(linebmp);
                linePred.PredictedText = "(Placeholder)";
                Thread.Sleep(2000);
                linePred.IsReady = true;                
                Debug.WriteLine($"Predicted text: {linePred.PredictedText}");
                MainThread.InvokeOnMainThreadAsync(() => PreviewLinePredictionList.RefreshItem(linePred));
                finished.Increment();
                SetProgressBarPercentage(count == 0 ? 1 : finished.Get() * 100.0 / count);
            });

            MainThread.BeginInvokeOnMainThread(() => ProcessingState = ProcessingState.Done);
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
            MainThread.InvokeOnMainThreadAsync(() => SetVerticalViewSplit(1));

            Task.Run(async () =>
            {
                var captureBytes = newValue;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ProcessingState = ProcessingState.Pending;
                    ProgressBar.Percentage = 0;
                    PreviewLinePredictionList.Clear();
                });
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
            await MainThread.InvokeOnMainThreadAsync(() => MaskImage.Source = null);
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

        private Logger Log = Logger.Create<ProcessingPage>();

        private async void FinalConfirmButton_Clicked(object sender, System.EventArgs e)
        {
            Log?.WriteLine("FinalConfirmButton_Clicked");

            await MainThread.InvokeOnMainThreadAsync(() => PreviewLinePredictionList.BeginRefresh());

            var imageStorage = Application.Current.As<App>().ImageStorage;
            var repository = Application.Current.As<App>().PredictionRepository;
            var imageRef = await imageStorage.MakeImageRef(Image.Source);


            Log?.WriteLine("Building predictedLines");

            var predictedLines = new List<PredictedLine>();
            foreach(var item in PreviewLinePredictionList.GetItems())
            {
                var line = new PredictedLine
                {
                    PredictedText = item.PredictedText,
                    SegmentedLine = await imageStorage.MakeImageRef(item.LineImage)
                };
                predictedLines.Add(line);
            }

            Log?.WriteLine($"Lines count = {predictedLines.Count}");
            Log?.WriteLine("Building prediction");
            var prediction = new Prediction
            {
                Date = DateTime.Now,
                Image = imageRef,
                PredictedLines = predictedLines
            };
            repository.Add(prediction);

            await MainThread.InvokeOnMainThreadAsync(() => PreviewLinePredictionList.EndRefresh());
            await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//PredictionsListPage"));
        }

        private void FinalRetryButton_Clicked(object sender, System.EventArgs e)
        {
            MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//NewCapturePage"));
        }
    }
}