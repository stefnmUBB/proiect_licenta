using Licenta.Commons.Math;
using Licenta.Utils;
using System.Drawing;

namespace Licenta.Imaging
{
    public class Image24 : Matrix<Color24>
    {
        public int Width => ColumnsCount;
        public int Height => RowsCount;

        public Image24(int width, int height) : base(height, width) { }
        public Image24(Bitmap bitmap) : base(bitmap.Height, bitmap.Width, bitmap.GetColorsFromBitmap()) { }    
    }
}
