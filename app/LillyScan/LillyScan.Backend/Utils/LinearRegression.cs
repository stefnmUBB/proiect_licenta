using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Utils
{
    public static class LinearRegression
    {
        public static (float B0, float B1, float d) LeastSquare2D(List<int> x, List<int> y)
        {
            long sxx = 0, sx = 0, sxy = 0, sy = 0;
            int n = x.Count;
            for (int i = 0; i < n; i++) 
            {
                sxx += x[i] * x[i];
                sx += x[i];
                sxy += x[i] * y[i];
                sy += y[i];
            }

            long d = sxx * n - sx * sx;
            if (d == 0)
                return (0, sy / n, 0);

            // sxx * b0 + sx * b1 = sxy | n
            // sx * b0 + n * b1 = sy    | sx

            // sxx*n * b0 + sx*n*b1 = sxy*n;
            // sx*sx*b0 + sx*n*n1 = sx*sy

            // d * b0 = (sxy*n - sx*sy)

            float b0 = (float)(1.0 * (sxy * n - sx * sy) / d);
            float b1 = (float)(1.0 * (sy - sx * b0) / n);

            return (b0, b1, 1.0f * d / (n * n));
        }
    }
}
