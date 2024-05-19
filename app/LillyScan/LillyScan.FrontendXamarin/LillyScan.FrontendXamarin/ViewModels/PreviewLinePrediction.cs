using Xamarin.Forms;

namespace LillyScan.FrontendXamarin.ViewModels
{
    public class PreviewLinePrediction
    {
        public ImageSource LineImage { get; set; }
        public string PredictedText { get; set; }
        public bool IsReady { get; set; } = false;
        public bool IsWorking => !IsReady;

        public override string ToString() => PredictedText;        
    }
}
