using LillyScan.Backend;
using LillyScan.Backend.API;

namespace LillyScan.FrontendXamarin.Utils
{
    public static class HTR
    {
        private static readonly HTREngine _Engine = new DefaultHTREngine();
        public static HTREngine Engine => _Engine;
    }
}
