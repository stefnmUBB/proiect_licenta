using LillyScan.Backend.Imaging;
using LillyScan.Backend.Utils;

namespace LillyScan.Backend.HTR
{
    public interface IHTREngine
    {
        RawBitmap SegmentTiles64(RawBitmap bitmap, SegmentationType segmentationType, bool parallel = true,
            bool resizeToOriginal = true,
            ProgressMonitor progressMonitor = null, string taskName = null);
        LocalizedMask[] SegmentLines(RawBitmap bitmap, ProgressMonitor progressMonitor = null,
            bool resizeToOriginal = true, string taskName = null);

        string PredictTextLine(RawBitmap bitmap, ProgressMonitor progressMonitor = null, string taskName = null);
    }
}
