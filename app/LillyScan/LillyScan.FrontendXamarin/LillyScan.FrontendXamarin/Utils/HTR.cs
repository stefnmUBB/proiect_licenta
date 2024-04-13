using LillyScan.Backend.API;

namespace LillyScan.FrontendXamarin.Utils
{
    public static class HTR
    {
        private static readonly DefaultHTREngine _Engine = new DefaultHTREngine();
        public static DefaultHTREngine Engine => _Engine;
    }
}
