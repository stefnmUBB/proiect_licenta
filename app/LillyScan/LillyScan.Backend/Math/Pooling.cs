using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Math
{
    internal static class Pooling
    {
        public static float MaxKernel(float[] values, int length)
        {
            if (length == 0) return 0;
            float m = values[0];
            for (int i = 0; i < length; i++)
                if (values[i] > m) m = values[i];
            return m;
        }

        public static (int N, int M) GetPooling2DValidPaddingDims(int N, int M, int poolSizeRows, int poolSizeCols, int strideRows, int strideCols)
            => ((N - poolSizeRows) / strideRows + 1, (M - poolSizeCols) / strideCols + 1);

        public static (int N, int M) GetPooling2DSamePaddingDims(int N, int M, int poolSizeRows, int poolSizeCols, int strideRows, int strideCols)
            => ((N - 1) / strideRows + 1, (M - 1) / strideCols + 1);

        public static unsafe void Pooling2DValidPadding(float[] a, float[] r, int B, int N, int M, int C,
            int poolSizeRows, int poolSizeCols, int stridesRows, int stridesCols, Func<float[], int, float> kernel)
        {
            fixed (float* pa = &a[0])
            fixed (float* pr = &r[0])
                Pooling2DValidPadding(pa, pr, B, N, M, C, poolSizeRows, poolSizeCols, stridesRows, stridesCols, kernel);
        }

        public static unsafe void Pooling2DValidPadding(float* a, float* r, int B, int N, int M, int C,
            int poolSizeRows, int poolSizeCols, int stridesRows, int stridesCols, Func<float[], int, float> kernel)
        {
            (int h, int w) = GetPooling2DValidPaddingDims(N, M, poolSizeRows, poolSizeCols, stridesRows, stridesCols);            
            var pool = new float[poolSizeRows * poolSizeCols];
            int poolLen = 0;

            int n0 = poolSizeRows / 2, n1 = N - (poolSizeRows - poolSizeRows / 2), ni = stridesRows;
            int m0 = poolSizeCols / 2, m1 = M - (poolSizeCols - poolSizeCols / 2), mi = stridesCols;

            int NMC = N * M * C;
            int MC = M * C;

            for (int b=0;b<B;b++)
            {
                for (int n = n0; n <= n1; n += ni)
                {
                    for (int m = m0; m <= m1; m += mi) 
                    {
                        for(int c=0;c<C;c++)
                        {
                            poolLen = 0;
                            for (int ii = n - poolSizeRows / 2; ii < n + (poolSizeRows - poolSizeRows / 2); ii++)
                            {                                
                                for (int jj = m - poolSizeCols / 2; jj < m + (poolSizeCols - poolSizeCols / 2); jj++)
                                {
                                    var value = a[b * NMC + ii * MC + jj * C + c];
                                    pool[poolLen++] = value;
                                }
                            }
                            *r++ = kernel(pool, poolLen);                            
                        }
                    }
                }
            }
        }

        public static unsafe void Pooling2DSamePadding(float[] a, float[] r, int B, int N, int M, int C,
            int poolSizeRows, int poolSizeCols, int stridesRows, int stridesCols, Func<float[], int, float> kernel)
        {
            fixed (float* pa = &a[0])
            fixed (float* pr = &r[0])
                Pooling2DSamePadding(pa, pr, B, N, M, C, poolSizeRows, poolSizeCols, stridesRows, stridesCols, kernel);
        }

        public static unsafe void Pooling2DSamePadding(float* a, float* r, int B, int N, int M, int C,
            int poolSizeRows, int poolSizeCols, int stridesRows, int stridesCols, Func<float[], int, float> kernel)
        {
            (int h, int w) = GetPooling2DSamePaddingDims(N, M, poolSizeRows, poolSizeCols, stridesRows, stridesCols);
            var pool = new float[poolSizeRows * poolSizeCols];
            int poolLen = 0;
                        

            int NMC = N * M * C;
            int MC = M * C;

            int offsetN = 1 - poolSizeRows % 2;
            int offsetM = 1 - poolSizeCols % 2;

            for (int b = 0; b < B; b++)
            {
                for (int n = 0; n < N; n += stridesRows)
                {
                    for (int m = 0; m < M; m += stridesCols)
                    {
                        for (int c = 0; c < C; c++)
                        {
                            poolLen = 0;
                            for (int ii = n - poolSizeRows / 2 + offsetN; ii < n + (poolSizeRows - poolSizeRows / 2) + offsetN; ii++) 
                            {
                                if (ii < 0 || ii >= N) continue;
                                for (int jj = m - poolSizeCols / 2 + offsetM; jj < m + (poolSizeCols - poolSizeCols / 2) + offsetM; jj++) 
                                {
                                    if (jj < 0 || jj >= M) continue;
                                    var value = a[b * NMC + ii * MC + jj * C + c];
                                    pool[poolLen++] = value;
                                }
                            }
                            *r++ = kernel(pool, poolLen);
                        }
                    }
                }
            }
        }


    }
}
