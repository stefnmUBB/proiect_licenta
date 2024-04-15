using LillyScan.Backend.Parallelization;
using LillyScan.Backend.Utils;

namespace LillyScan.FrontendXamarin.Utils
{
    public static class AppState
    {
        public static Observable<byte[]> CaptureBytes = new Observable<byte[]>();
        //public static Atomic<byte[]> CaptureBytes = new Atomic<byte[]>();
    }
}
