using LillyScan.Backend.Imaging;
using LillyScan.Backend.Math;
using LillyScan.Backend.Utils;
using System;

namespace LillyScan.Backend
{
    public abstract class HTREngine : IHTREngine
    {
        public abstract string Predict(IReadMatrix<double> image);
        public string Predict(ImageRGB image) => Predict(ToGrayscaleDefault(image));
        private static Matrix<double> ToGrayscaleDefault(ImageRGB img)
            => Matrices.DoEachItem(img, x => (x.R.Value + x.G.Value + x.B.Value) / 3);
        public unsafe virtual byte[] Segment(byte[] image)
        {
            if (image.Length != 256 * 256)
                throw new ArgumentException($"DefaultHTREngine.Segment: Invalid input length");

            var normalized = new float[256 * 256];

            fixed (byte* s = &image[0])
            fixed (float* d = &normalized[0])
            {
                var si = s;
                var di = d;
                for (int i = 0; i < 256 * 256; i++)
                    *di++ = *si++ / 255.0f;
            }

            var predicted = Segment(normalized);
            var result = new byte[256 * 256 * 3];

            fixed (float* s = &predicted[0]) 
            fixed (byte* d = &result[0])
            {
                var si = s;
                var di = d;
                for (int i = 0; i < 256 * 256; i++)
                    *di++ = (byte)((*si++).Clamp(0, 1) * 255);
            }
            return result;
        }

        public abstract float[] Segment(float[] image);

        public abstract float[] Segment64(float[] image);        
    }
}
