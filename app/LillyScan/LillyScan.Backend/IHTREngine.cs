using LillyScan.Backend.Imaging;
using LillyScan.Backend.Math;
using LillyScan.Backend.Utils;

namespace LillyScan.Backend
{
    public interface IHTREngine
    {
        RawBitmap SegmentTiles64(RawBitmap bitmap, bool preview = false, bool parallel=true, ProgressMonitor progressMonitor = null);
        RawBitmap SegmentFix(RawBitmap image, RawBitmap mask, ProgressMonitor progressMonitor = null);


        float[] Segment(float[] image);
        float[] Segment64(float[] image, bool preview=false);
        byte[] Segment(byte[] image);
        string Predict(IReadMatrix<double> image);
        string Predict(ImageRGB image);
    }
}
