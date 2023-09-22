using Licenta.Commons.Math;
using Licenta.Commons.Math.Arithmetics;
using Licenta.Commons.Utils;
using Licenta.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licenta.Utils
{
    public static class Images
    {
        public static Matrix<DoubleNumber> ToGrayScaleMatrix(this ImageRGB img)
        {
            return Matrices.DoEachItem(img, c => (DoubleNumber)(0.299 * c.R.Value + 0.587 * c.G.Value + 0.114 * c.B.Value));
        }

        public static ImageRGB CannyEdgeDetection(this ImageRGB img)
        {
            var wimg = img.ToGrayScaleMatrix();
            wimg = Matrices.Convolve(wimg, Kernels.GaussianFilter(5, 5, 1.0));
            var gradX = Matrices.Convolve(wimg, Kernels.SobelX());
            var gradY = Matrices.Convolve(wimg, Kernels.SobelY());
            var magn = Matrices.DoItemByItem(gradX, gradY, Functions.Hypot);
            var dir = Matrices.DoItemByItem(gradY, gradX, Functions.Atan2);

            (int X, int Y)[] dirs = { (1, 0), (1, 1), (0, 1), (-1, -1), (-1, 0) };

            (int X, int Y) getGradientDirection(double angle)
            {                
                var i = (int)(angle * 4 / Math.PI);
                var d = dirs[(i < 0 ? i : -i).Clamp(0, 4)];
                if (i < 0) d.Y = -d.Y;
                return d;
            }

            for(int r=0;r<magn.RowsCount;r++)
            {
                for(int c=0;c<magn.ColumnsCount;c++)
                {
                    var d = getGradientDirection(dir[r, c].Value);
                    var r2 = r + d.Y;
                    var c2 = c + d.X;
                    if(0<=r2 && r2<magn.RowsCount && 0<=c2 && c2<magn.ColumnsCount)
                    {
                        if (magn[r,c].Value < magn[r2,c2].Value)
                        {
                            magn[r, c] = new DoubleNumber(0);
                            continue;
                        }
                    }
                    d = (-d.X, -d.Y);
                    r2 = r + d.Y;
                    c2 = c + d.X;
                    if (0 <= r2 && r2 < magn.RowsCount && 0 <= c2 && c2 < magn.ColumnsCount)
                    {
                        if (magn[r, c].Value < magn[r2, c2].Value)
                        {
                            magn[r, c] = new DoubleNumber(0);
                            continue;
                        }
                    }
                }
            }

            magn = Matrices.ApplyDoubleThreshold(magn, 0, 1, 0.2, 0.8);

            new ImageRGB(magn).ToBitmap().Save("magn.png");
            new ImageRGB(dir).ToBitmap().Save("dir.png");

            return new ImageRGB(dir);
        }

    }
}
