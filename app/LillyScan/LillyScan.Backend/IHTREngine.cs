using LillyScan.Backend.Imaging;
using LillyScan.Backend.Math;

namespace LillyScan.Backend
{
    public interface IHTREngine
    {
        float[] Segment(float[] image);
        byte[] Segment(byte[] image);
        string Predict(IReadMatrix<double> image);
        string Predict(ImageRGB image);
    }
}
