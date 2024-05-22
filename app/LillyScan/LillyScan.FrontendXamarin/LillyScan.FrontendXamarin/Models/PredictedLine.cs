using System.IO;

namespace LillyScan.FrontendXamarin.Models
{
    public class PredictedLine
    {
        public ImageRef SegmentedLine { get; set; }
        public string PredictedText { get; set; }

        public static PredictedLine ReadBinary(BinaryReader br)
        {
            var segmentedLine = ImageRef.ReadBinary(br);
            var predictedText = br.ReadString();
            return new PredictedLine
            {
                SegmentedLine = segmentedLine,
                PredictedText = predictedText
            };
        }

        public void WriteBinary(BinaryWriter bw)
        {
            SegmentedLine.WriteBinary(bw);
            bw.Write(PredictedText);
        }
    }
}
