using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LillyScan.Backend.AI.Layers
{
    internal static class UnsafeOperations
    {        
        public static unsafe void Conv2D(float* tbuff, float* kbuff, float* rbuff, int B, int N, int M, int C, int K1, int K2, int F)
        {
            int NMF = N * M * F;
            int MF = N * F;
            int NMC = N * M * C;
            int MC = M * C;
            int K2CF = K2 * C * F;
            int CF = C * F;

            var rangePartitioner = Partitioner.Create(0, N);

            for (int b = 0; b < B; b++)
            {
                int startC0 = b * NMC;
                int startF0 = b * NMF;

                Parallel.ForEach(rangePartitioner, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (range, loopState) =>
                {
                    for (int n = range.Item1; n < range.Item2; n++) 
                    //for (int n = 0; n < N; n++) 
                    {
                        int startF1 = startF0 + n * MF;                        

                        for (int m = 0; m < M; m++)
                        {
                            int startF = startF1 + m * F;
                            for (int k1 = 0; k1 < K1; k1++)
                            {
                                int ii = n + k1 - (K1 >> 1);
                                int startC1 = startC0 + ii * MC;
                                for (int k2 = 0; k2 < K2; k2++)
                                {
                                    int jj = m + k2 - (K2 >> 1);
                                    if (ii < 0 || jj < 0 || ii >= N || jj >= M)
                                        continue;
                                    int startC = startC1 + jj * C;
                                    int startK = k1 * K2CF + k2 * CF;

                                    for (int c = 0; c < C; c++)
                                    {
                                        int kx = startK + c * F;
                                        float el = tbuff[startC + c];
                                        for (int f = 0; f < F; f++)
                                        {
                                            rbuff[startF + f] += el * kbuff[kx + f];
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                );
            }
        }
        public static unsafe void Conv2DTranspose(float* tbuff, float* kbuff, float* rbuff, int B, int N, int M, int C, int K1, int K2, int F, int strideRows, int strideCols)
        {
            var outH = strideRows * N;
            var outW = strideCols * M;

            int NMC = N * M * C;
            int NMF = N * M * F;
            int MC = M * C;
            int MF = M * F;
            int K2FC = K2 * F * C;
            int FC = F * C;
            int k1off = (K1 - strideRows + 1) / 2;
            int k2off = (K2 - strideRows + 1) / 2;

            var rangePartitioner = Partitioner.Create(0, F);

            for (int b = 0; b < B; b++)
            {
                float* imat = tbuff + b * NMC;
                float* omat = rbuff + b * outH * outW * F;
                for (int i = 0; i < N; i++)
                {
                    float* channelsLine = imat + i * MC;
                    for (int j = 0; j < M; j++)
                    {
                        float* channels = channelsLine + j * C;
                        for (int k1 = 0; k1 < K1; k1++)
                        {
                            int ii = i * strideRows + k1 - k1off;
                            if (ii < 0 || ii >= outH) continue;
                            for (int k2 = 0; k2 < K2; k2++)
                            {
                                float* kernelFC = kbuff + k1 * K2FC + k2 * FC;                                
                                int jj = j * strideCols + k2 - k2off;
                                if (jj < 0 || jj >= outW) continue;
                                var filters = unchecked(omat + ii * outW * F + jj * F);
                                for (int f = 0; f < F; f++)
                                {
                                    for (int c = 0; c < C; c++)
                                    {
                                        filters[f] += kernelFC[f * C + c] * channels[c];
                                    }
                                }                                                                
                            }
                        }
                    }
                }
            }
        }
        public static unsafe void MatMul(float* a, float* b, float* r, int m, int n, int p)
        {
            var rangePartitioner = Partitioner.Create(0, m);
            Parallel.ForEach(rangePartitioner, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (range, loopState) =>
            {
                for (int i = range.Item1; i < range.Item2; i++) 
                //for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        for (int k = 0; k < p; k++)
                        {
                            r[i * p + k] += a[i * n + j] * b[j * p + k];
                        }
                    }
                }
            });
        }        

        public static unsafe void UpSampling2D(float* tbuff, float* rbuff, int B, int N, int M, int C, int scaleRows, int scaleCols)
        {
            int DN = scaleRows * N;
            int DM = scaleCols * M;            
            for (int b = 0; b < B; b++) 
            {
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < M; j++)
                    {
                        int idst = b * DN * DM * C + i * scaleRows * DM * C + j * scaleCols * C;
                        for (int c = 0; c < C; c++)
                        {
                            float value = *tbuff++;
                            for (int p = 0; p < scaleRows; p++)
                            {
                                for (int q = 0; q < scaleCols; q++)
                                {
                                    var index = idst + p * DM * C + q * C + c;
                                    rbuff[index] = value;
                                }
                            }                            
                        }
                    }
                }
            }
        }
    }
}
