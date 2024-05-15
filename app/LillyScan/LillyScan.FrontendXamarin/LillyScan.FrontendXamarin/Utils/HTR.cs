using LillyScan.Backend.HTR;

namespace LillyScan.FrontendXamarin.Utils
{
    public static class HTR
    {
        private static readonly IHTREngine _Engine = new BuiltInHTREngine();
        public static IHTREngine Engine => _Engine;
    }
}
