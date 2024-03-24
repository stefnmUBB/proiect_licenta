using LillyScan.Backend.Math.Arithmetics.BuiltInTypeWrappers;
using LillyScan.Backend.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Imaging
{
    public class ImageRGB : Matrix<ColorRGB>
    {
        public int Width => ColumnsCount;
        public int Height => RowsCount;

        public ImageRGB(int width, int height) : base(height, width) { }                
        public ImageRGB(IReadMatrix<ColorRGB> m) : base(m) { }
        public ImageRGB(IReadMatrix<DoubleNumber> m) : base(m.Select(v => new ColorRGB(v, v, v))) { }
        public ImageRGB(IReadMatrix<double> m) : base(m.Select(v => new ColorRGB(v, v, v))) { }
        public ImageRGB(IReadMatrix<byte> m) : base(m.Select(v => new ColorRGB(v, v, v))) { }               
    }
}
