using HelpersCurveDetectorDataSetGenerator.Commons.Math;
using HelpersCurveDetectorDataSetGenerator.Commons.Math.Arithmetics;
using HelpersCurveDetectorDataSetGenerator.Utils;
using System.Drawing;

namespace HelpersCurveDetectorDataSetGenerator.Imaging
{
    public class ImageRGB : Matrix<ColorRGB>
    {
        public int Width => ColumnsCount;
        public int Height => RowsCount;

        public ImageRGB(int width, int height) : base(height, width) { }
        public ImageRGB(Bitmap bitmap) : base(bitmap.Height, bitmap.Width, bitmap.GetColorsFromBitmap()) { }
        public ImageRGB(IReadMatrix<ColorRGB> m) : base(m) { }

        public ImageRGB(IReadMatrix<DoubleNumber> m) : base(m.Select(v => new ColorRGB(v, v, v))) { }

        public ImageRGB Convolve(Matrix<DoubleNumber> c) 
            => new ImageRGB(Matrices.Convolve<ColorRGB, DoubleNumber, ColorRGB>(this, c));        
    }
}
