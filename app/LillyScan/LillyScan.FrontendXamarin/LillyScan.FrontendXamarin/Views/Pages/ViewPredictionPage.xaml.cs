using LillyScan.Backend.Parallelization;
using LillyScan.FrontendXamarin.Repository;
using LillyScan.FrontendXamarin.Utils;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin.Views.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewPredictionPage : ContentPage
    {
        private readonly ImageStorage ImageStorage;
        private readonly PredictionRepository Repository;

        public ViewPredictionPage()
        {
            InitializeComponent();
            ImageStorage = Application.Current.As<App>().ImageStorage;
            Repository = Application.Current.As<App>().PredictionRepository; 
        }

        Logger Log = Logger.Create<ViewPredictionPage>();

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var prediction = AppState.SelectedPrediction.Value;
            if (prediction!=null)
            {
                Task.Run(async () =>
                {
                    MainThread.BeginInvokeOnMainThread(() => SetVerticalViewSplit(1));

                    var imageSource = await ImageStorage.FetchImage(prediction.Image);
                    MainThread.BeginInvokeOnMainThread(() => Image.Source = imageSource);
                    MainThread.BeginInvokeOnMainThread(() => PreviewLinePredictionList.Clear());

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        PreviewLinePredictionList.Clear();
                        AnimateVerticalSplit(0.5);
                        PreviewLinePredictionList.BeginRefresh();
                    });
                    Log?.WriteLine("Loading lines");
                    Repository.LoadPredictedLines(prediction);
                    Log?.WriteLine($"Found {prediction.PredictedLines.Count} lines");
                    for (int i = 0; i < prediction.PredictedLines.Count; i++) 
                    {                        
                        var lineImage = await ImageStorage.FetchImage(prediction.PredictedLines[i].SegmentedLine);
                        var predText = prediction.PredictedLines[i].PredictedText;

                        var item = new ViewModels.PreviewLinePrediction
                        {
                            IsReady = true,
                            LineImage = lineImage,
                            PredictedText = predText
                        };
                        Log?.WriteLine($"Item: {lineImage.Id}/{predText}");

                        await MainThread.InvokeOnMainThreadAsync(() => PreviewLinePredictionList.AddItem(item));
                    }
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        PreviewLinePredictionList.EndRefresh();
                        AnimateVerticalSplit(0.5);
                    });
                });
            }            
        }

        protected override bool OnBackButtonPressed()
        {
            MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//PredictionsListPage"));
            return true;            
        }


        private Atomic<double> CurrentVerticalSplit = new Atomic<double>(1);
        private void SetVerticalViewSplit(double value)
        {
            AbsoluteLayout.SetLayoutBounds(PreviewMaskGrid, new Rectangle(0, 0, 1, value));
            AbsoluteLayout.SetLayoutBounds(PreviewLinePredictionList, new Rectangle(0, 1, 1, 1 - value));
            CurrentVerticalSplit.Set(value);
        }

        private void AnimateVerticalSplit(double toValue)
        {
            new Animation(SetVerticalViewSplit, CurrentVerticalSplit.Get(), toValue).Commit(ContentBox, "VerticalSplit");
        }




    }
}