using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace LineSegmentationDatasetCreator
{
    public class Polygon
    {
        public string Name { get; set; }
        public List<Point> Points = new List<Point>();

        public override string ToString() => Name;
    }
}
