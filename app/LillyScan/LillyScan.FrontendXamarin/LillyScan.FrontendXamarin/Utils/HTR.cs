using LillyScan.Backend;
using LillyScan.Backend.HTR;

namespace LillyScan.FrontendXamarin.Utils
{
    public static class HTR
    {
        private static readonly Backend.HTR.IHTREngine _Engine = new BuiltInHTREngine();
        public static Backend.HTR.IHTREngine Engine => _Engine;
    }
}
