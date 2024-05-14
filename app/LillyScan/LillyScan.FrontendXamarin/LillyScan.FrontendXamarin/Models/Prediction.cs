using System;
using System.Collections.Generic;

namespace LillyScan.FrontendXamarin.Models
{
    internal class Prediction
    {
        public ImageRef Image { get; set; }
        public DateTime Date { get; set; }
        public List<PredictedLine> PredictedLines { get; set; }
    }
}
