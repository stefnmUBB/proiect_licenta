using System;
using System.Collections.Generic;
using System.IO;

namespace LillyScan.FrontendXamarin.Models
{
    public class Prediction
    {
        public int Id { get; set; }
        public ImageRef Image { get; set; }
        public DateTime Date { get; set; }
        public List<PredictedLine> PredictedLines { get; set; }

    }
}
