using System;

namespace LillyScan.Backend.Math
{
    public static class PlatformConfig
    {
        public static Action<float[], float[], float[], int, int, int> DotMul = null;
        public static Conv2DMethod Conv2DMethod = Conv2DMethod.Img2Col;
    }

    public enum Conv2DMethod { Classic, Img2Col }
}
