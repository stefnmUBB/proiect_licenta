using LillyScan.Backend.AI.Layers;
using System;
using System.IO;

namespace LillyScan.Backend.Utils
{
    public static partial class QuickOps
    {
        public static unsafe void ForwardLSTM(float[] input, float[] output, float[] w, float[] u, float[] bias, int B, int T, int L,
            int units)
        {
            fixed (float* pinput = &input[0])
            fixed (float* poutput = &output[0])
            fixed (float* pw = &w[0])
            fixed (float* pu = &u[0])
            fixed (float* pb = &bias[0])
                ForwardLSTM(pinput, poutput, pw, pu, pb, B, T, L, units);
        }


        public static unsafe void ForwardLSTM(float* input, float* output, float* w, float* u, float* bias, int B, int T, int L,
            int units)
        {
            var c = new float[units];
            var h = new float[units];
            var rc = new float[units];
            var rh = new float[units];

            var tmp = new float[8 * units];
            fixed (float* ptmp = &tmp[0])
            fixed (float* pc = &c[0])
            fixed (float* ph = &h[0])
            fixed (float* prc = &rc[0])
            fixed (float* prh = &rh[0])
            {
                for (int b = 0; b < B; b++)
                {
                    for (int i = 0; i < units; i++) pc[i] = ph[i] = 0;                        
                    for (int t = 0; t < T; t++)
                    {
                        float* x = input + b * T * L + t * L;
                        Array.Clear(tmp, 0, tmp.Length);
                        ForwardLSTMStep(pc, ph, x, w, u, bias, prc, prh, ptmp, L, units);
                        for (int i = 0; i < units; i++) pc[i] = prc[i];
                        for (int i = 0; i < units; i++) ph[i] = prh[i];

                        for (int i = 0; i < units; i++) *output++ = ph[i];
                    }
                }
            }            
        }        

        public static unsafe void ForwardLSTMStep(float[] c, float[] h, float[] x, float[] w, float[] u, float[] b,
            float[] rc, float[] rh, float[] tmp,
            int L, int U)
        {
            fixed(float* pc = &c[0])
            fixed(float* ph = &h[0])
            fixed(float* px = &x[0])
            fixed(float* pw = &w[0])
            fixed(float* pu = &u[0])
            fixed(float* pb = &b[0])
            fixed(float* ptmp = &tmp[0])
            fixed(float* prc = &rc[0])
            fixed(float* prh = &rh[0])
            {
                ForwardLSTMStep(pc, ph, px, pw, pu, pb, prc, prh, ptmp, L, U);
            }
        }

        private static unsafe void WriteArray(StreamWriter w, float* x, int len)
        {
            for (int i = 0; i < len; i++) w.Write($"{x[i]}, ");
            w.WriteLine();
        }

        public static unsafe void ForwardLSTMStep(float* c, float* h, float* x, float* w, float* u, float* b, float* rc, float* rh, float* tmp,
            int L, int U)
        {            
            int tmpOffset = 0;
            float* t = &tmp[tmpOffset]; tmpOffset += 4 * U;
            UnsafeOperations.MatMul(x, w, t, 1, L, 4 * U, clearOutput: true);
            UnsafeOperations.MatMul(h, u, t, 1, U, 4 * U, clearOutput: false);
            if (b != null)
                UnsafeOperations.AddTo(b, t, 4 * U);            

            float* it = &t[0 * U], ft = &t[1 * U], ctt = &t[2 * U], ot = &t[3 * U];
            MapSigmoid(it, U);
            MapSigmoid(ft, U);
            MapTanh(ctt, U);
            MapSigmoid(ot, U);            

            float* ct = &tmp[tmpOffset]; tmpOffset += U;
            float* t2 = &tmp[tmpOffset]; tmpOffset += U;
            float* ht = &tmp[tmpOffset]; tmpOffset += U;
            VecElementWiseMul(ft, c, ct, U, addToExisting: false);
            VecElementWiseMul(it, ctt, ct, U, addToExisting: true);            

            CopyTo(ct, t2, U);
            MapTanh(t2, U);
            VecElementWiseMul(ot, t2, ht, U);

            CopyTo(ct, rc, U);
            CopyTo(ht, rh, U);
            
        }

        private static unsafe void CopyTo(float* a, float* r, int len)
        {
            for (int i = 0; i < len; i++) r[i] = a[i];
        }

        private static unsafe void VecElementWiseMul(float* a, float* b, float* r, int len, bool addToExisting = false)
        {
            if(addToExisting)
                for (int i = 0; i < len; i++) r[i] += a[i] * b[i];
            else
                for (int i = 0; i < len; i++) r[i] = a[i] * b[i];
        }

        private static unsafe void MapTanh(float* x, int len)
        {
            for (int i = 0; i < len; i++) x[i] = (float)System.Math.Tanh(x[i]);
        }

        private static unsafe void MapSigmoid(float* x, int len)
        {
            for (int i = 0; i < len; i++) x[i] = (float)(1 / (1 + System.Math.Exp(-x[i])));        
        }

    }
}
