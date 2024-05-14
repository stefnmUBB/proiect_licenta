using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.FrontendXamarin.Models
{
    public class PredictedLine
    {
        public ImageRef SegmentedLine { get; set; }
        public string PredictedText { get; set; }
    }
}
