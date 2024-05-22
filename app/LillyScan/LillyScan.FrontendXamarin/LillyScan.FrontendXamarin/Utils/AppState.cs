using LillyScan.Backend.Utils;
using LillyScan.FrontendXamarin.Models;

namespace LillyScan.FrontendXamarin.Utils
{
    public static class AppState
    {
        public static Observable<byte[]> CaptureBytes = new Observable<byte[]>();
        //public static Atomic<byte[]> CaptureBytes = new Atomic<byte[]>();

        public static Observable<Prediction> SelectedPrediction = new Observable<Prediction>();

    }
}
