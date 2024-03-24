using LillyScan.Backend.Imaging;
using LillyScan.Backend.Math;

namespace LillyScan.Backend
{
    public abstract class HTREngine : IHTREngine
    {
        public abstract string Predict(IReadMatrix<double> image);
        public string Predict(ImageRGB image) => Predict(ToGrayscaleDefault(image));
        private static Matrix<double> ToGrayscaleDefault(ImageRGB img)
            => Matrices.DoEachItem(img, x => (x.R.Value + x.G.Value + x.B.Value) / 3);
    }
}
