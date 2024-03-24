using LillyScan.Backend.Imaging;
using LillyScan.Backend.Math;

namespace LillyScan.Backend
{
    public interface IHTREngine
    {
        string Predict(IReadMatrix<double> image);
        string Predict(ImageRGB image);
    }
}
