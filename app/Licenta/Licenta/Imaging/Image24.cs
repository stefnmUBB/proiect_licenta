using Licenta.Commons.Math;
using Licenta.Utils;
using System.Drawing;

namespace Licenta.Imaging
{
    public class Image24 : Matrix<Color24>
    {
        public Image24(int rowsCount, int columnsCount) : base(rowsCount, columnsCount) { }        
        public Image24(Bitmap bitmap) : base(bitmap.Width, bitmap.Height, bitmap.GetColorsFromBitmap()) { }       
    }
}
