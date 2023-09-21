using Licenta.Commons.Math;
using Licenta.Commons.Math.Arithmetics;
using Licenta.Utils;
using System.Drawing;

namespace Licenta.Imaging
{
    public class ImageRGB : Matrix<ColorRGB>
    {
        public int Width => ColumnsCount;
        public int Height => RowsCount;

        public ImageRGB(int width, int height) : base(height, width) { }
        public ImageRGB(Bitmap bitmap) : base(bitmap.Height, bitmap.Width, bitmap.GetColorsFromBitmap()) { }
        public ImageRGB(IReadMatrix<ColorRGB> m) : base(m) { }
        public ImageRGB Convolute(Matrix<DoubleNumber> c) 
            => new ImageRGB(Matrices.Convolute<ColorRGB, DoubleNumber, ColorRGB>(this, c));
    }
}
