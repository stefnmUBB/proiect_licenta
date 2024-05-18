using Xamarin.Forms;

namespace LillyScan.FrontendXamarin.ViewModels
{
    class PreviewLinePrediction
    {
        public ImageSource LineImage { get; set; }
        public string PredictedText { get; set; }
        public bool IsReady { get; set; } = false;

        public override string ToString() => PredictedText;        
    }
}
