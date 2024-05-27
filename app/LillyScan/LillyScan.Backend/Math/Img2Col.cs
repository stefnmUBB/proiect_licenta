using LillyScan.Backend.AI.Layers;
using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace LillyScan.Backend.Math
{
    public static class Img2Col
    {
        private static unsafe void I2C_TransformSource(float* buf, float* res, int B, int N, int M, int C, int K1, int K2)
        {
            int NMC = N * M * C, MC = M * C;
            int NM = N * M;
            int NMCK1K2 = N * M * C * K1 * K2;
            int CK1K2 = C * K1 * K2;
            int hK1 = (K1 >> 1) - (1 - K1 % 2);
            int hK2 = (K2 >> 1) - (1 - K2 % 2);
            int K2C = K2 * C;
            int Cd8 = C / 8;
            int Cm8 = C % 8;

            for (int b = 0; b < B; b++)
            {                
                for (int n = 0; n < N; n++)
                {
                    for (int m = 0; m < M; m++)
                    {                        
                        for (int k1 = 0; k1 < K1; k1++)
                        {
                            int ii = n + k1 - hK1;
                            if (ii < 0 || ii >= N)
                            {
                                for (int q = 0; q < K2C; q++) *res++ = 0;                                
                            }
                            else
                            {
                                for (int k2 = 0; k2 < K2; k2++)
                                {
                                    int jj = m + k2 - hK2;
                                    if (jj < 0 || jj >= M) 
                                    {
                                        for (int c = 0; c < C; c++) *res++ = 0;
                                    }
                                    else
                                    {
                                        float* src = buf + b * NMC + ii * MC + jj * C;
                                        for(int c=Cd8;c>0;c--)
                                        {
                                            *res++ = *src++; *res++ = *src++; *res++ = *src++; *res++ = *src++;
                                            *res++ = *src++; *res++ = *src++; *res++ = *src++; *res++ = *src++;
                                        }
                                        for (int c = Cm8; c > 0; c--)
                                            *res++ = *src++;

                                        //for (int c = 0; c < C; c++) *res++ = *src++;
                                    }
                                }
                            }
                        }                             
                    }
                }
            }
        }

        private static unsafe void I2C_TransformKernel(float* buf, float* res, int K1, int K2, int C, int F)
        {
            int CK1K2 = C * K1 * K2;
            int K1K2 = K1 * K2;
            for (int c = 0; c < C; c++)
                for (int k1 = 0; k1 < K1; k1++)
                    for (int k2 = 0; k2 < K2; k2++)
                        for (int f = 0; f < F; f++)
                        {
                            res[f * CK1K2 + c * K1K2 + k1 * K2 + k2] = *buf++;
                        }
        }

        private static unsafe void MatMulBatch(float* a, float* b, float* r, int B, int M, int N, int P)
        {
            for(int i=0;i<B;i++)
            {
                UnsafeOperations.MatMul(a, b + i * M * N, r + i * M * P, M, N, P);
            }
        }

        private static unsafe void DotMulBatch(float* a, float* b, float* r, int B, int RA, int RB, int C)
        {
            for (int i = 0; i < B; i++)
            {
                DotMul(a, b + i * RB * C, r + i * RA * RB, RA, RB, C);
            }
        }

        private static unsafe void DotMul(float* a, float* b, float* r, int RA, int RB, int C)
        {
            int Cd8 = C / 8, Cm8 = C % 8;
            for (int j = 0; j < RB; j++)
                for (int i = 0; i < RA; i++)                                           
                {
                    float* s1 = a + C * i;
                    float* s2 = b + C * j;
                    float s = 0;

                    /*for (int c = Cd8; c > 0; c--) 
                    {
                        s += (*s1++) * (*s2++); s += (*s1++) * (*s2++); s += (*s1++) * (*s2++); s += (*s1++) * (*s2++);
                        s += (*s1++) * (*s2++); s += (*s1++) * (*s2++); s += (*s1++) * (*s2++); s += (*s1++) * (*s2++);
                    }*/
                    for (int c = C; c > 0; c--) 
                        s += (*s1++) * (*s2++);
                    *r++ = s;
                }                        
        }

        public static Stopwatch sw = new Stopwatch();

        public static unsafe void Conv2D(float* tbuff, float* kbuff, float* rbuff, int B, int N, int M, int C, int K1, int K2, int F)
        {
            var matK = new float[K1 * K2 * C * F];
            var matT = new float[B * K1 * K2 * C * N * M];
            var matR = new float[B * N * M * F];
            fixed (float* pmatK = &matK[0])
            fixed (float* pmatT = &matT[0])
            fixed (float* pmatR = &matR[0]) 
            {
                //sw?.Restart();
                I2C_TransformKernel(kbuff, pmatK, K1, K2, C, F);
                I2C_TransformSource(tbuff, pmatT, B, N, M, C, K1, K2);
                var el1 = sw?.Elapsed.TotalMilliseconds ?? 0.0;
                //sw?.Restart();
                if (PlatformConfig.DotMul != null)
                    PlatformConfig.DotMul(matK, matT, matR, F, N * M, K1 * K2 * C);
                else
                    DotMulBatch(pmatK, pmatT, pmatR, B, F, N * M, K1 * K2 * C);
                //sw?.Stop();
                //var el2 = sw?.Elapsed.TotalMilliseconds ?? 0.0;                
                //Debug.WriteLine($"Img2ColConv:");
                //Debug.WriteLine($"Transform: {el1}");
                //Debug.WriteLine($"Mul: {el2}");
                for (int i = 0, l = matR.Length; i < l; i++) rbuff[i] = matR[i];
            }            
        }

        public static unsafe void Conv2D(float[] tbuff, float[] kbuff, float[] rbuff, int B, int N, int M, int C, int K1, int K2, int F)
        {
            fixed (float* ptbuff = &tbuff[0])
            fixed (float* pkbuff = &kbuff[0])
            fixed (float* prbuff = &rbuff[0])
                Conv2D(ptbuff, pkbuff, prbuff, B, N, M, C, K1, K2, F);
            
        }

        public static unsafe void Run()
        {
            int B = 1, N = 256, M = 256, C = 64, F = 128, K1 = 3, K2 = 3;
            var t = new float[B * N * M * C];
            var k = new float[K1*K2*C*F];
            var o = new float[B * N * M * F];

            var r = new Random();
            for (int i = 0; i < t.Length; i++) t[i] = (float)r.NextDouble();
            for (int i = 0; i < k.Length; i++) t[i] = (float)r.NextDouble();

            Conv2D(t, k, o, B, N, M, C, K1, K2, F);
        }

        public static unsafe void Run0()
        {            
            int B=1, N = 4, M = 4;
            int K1 = 2, K2 = 2, C = 2, F = 2;
            var k = new float[K1 * K2 * C * F];
            for (int i = 0; i < k.Length; i++) k[i] = i + 1;
            new Tensor<float>((K1, K2, C, F), k).Print();
            var r = new float[F * K1 * K2 * C];
            fixed (float* pk = &k[0])
            fixed (float* pr = &r[0])
                I2C_TransformKernel(pk, pr, K1, K2, C, F);
            new Tensor<float>((F, K1 * K2 * C), r).Print();

            var s = new float[B * N * M * C];
            for (int i = 0; i < s.Length; i++) s[i] = i + 1;
            new Tensor<float>((B,N,M,C), s).Print();

            var t = new float[B * K1 * K2 * C * N * M];
            fixed (float* ps = &s[0])
            fixed (float* pt = &t[0])
                I2C_TransformSource(ps, pt, B, N, M, C, K1, K2);
            new Tensor<float>((B, N * M, K1 * K2 * C), t).Print();

            var o = new float[B * N * M * F];
            for (int i = 0; i < o.Length; i++) o[i] = 0;
            fixed (float* pt = &t[0])
            fixed (float* pr = &r[0])
            fixed (float* po = &o[0])
            {
                DotMulBatch(pr, pt, po, B, F, M * N, K1 * K2 * C);
                //MatMulBatch(pr, pt, po, B, F, K1 * K2 * C, M * N);
            }


            new Tensor<float>((B, F, N * M), o).Print("out=");

            var oo = new float[B * N * M * F];
            int NMF = N * M * F, MF = M * F, NM = N * M;
            for(int b=0;b<B;b++)
            {
                for(int i=0;i<F;i++)
                    for(int j=0;j<N*M;j++)
                    {
                        oo[b * NMF + j * F + i] = o[b * NMF + i * NM + j];
                    }
            }

            new Tensor<float>((B, N * M, F), oo).Print("oo=");
        }

    }
}
