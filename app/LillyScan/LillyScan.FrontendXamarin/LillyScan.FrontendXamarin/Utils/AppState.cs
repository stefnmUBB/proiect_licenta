using LillyScan.Backend.Parallelization;

namespace LillyScan.FrontendXamarin.Utils
{
    public static class AppState
    {
        public static Atomic<byte[]> CaptureBytes = new Atomic<byte[]>();
    }
}
