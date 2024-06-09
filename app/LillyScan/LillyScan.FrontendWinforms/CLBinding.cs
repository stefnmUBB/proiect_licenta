using Cloo;
using Cloo.Bindings;
using LillyScan.Backend.Math;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LillyScan.FrontentWinforms
{ 
    public static class CLBinding
    {
        private static ComputePlatform Platform;
        private static ComputeContext Context;
        private static ComputeProgram Program;        
        private static ComputeKernel DotMulKernel;

        private static string ProgramBuildFlags = "-cl-fast-relaxed-math -cl-no-signed-zeros";
        public static void Init()
        {
            Debug.WriteLine("Platform");
            Platform = ComputePlatform.Platforms[0];
            Debug.WriteLine("Context");
            Context = new ComputeContext(ComputeDeviceTypes.Gpu, new ComputeContextPropertyList(Platform), null, IntPtr.Zero);
            Debug.WriteLine("Program");
            Program = new ComputeProgram(Context, KernelCode);            
            Program.Build(null, ProgramBuildFlags, null, IntPtr.Zero);            
            Debug.WriteLine("Kernel");            
            DotMulKernel = Program.CreateKernel("DotMul");
            Debug.WriteLine("Done");
        }
   
        public static void DotMul(float[] a, float[] b, float[] r, int RA, int RB, int C)
        {
            if (RA*RB % 4 != 0) 
            {
                Debug.WriteLine($"DotMul: RA*RB is not multiple of 4: {RA}*{RB}");
                throw new ArgumentException($"DotMul: RA*RB is not multiple of 4: {RA}*{RB}");
            }
            using (var aBuffer = new ComputeBuffer<float>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, a))
            using (var bBuffer = new ComputeBuffer<float>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, b))
            using (var rBuffer = new ComputeBuffer<float>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, r))
            using (var queue = new ComputeCommandQueue(Context, Context.Devices[0], ComputeCommandQueueFlags.None))
            using (var kernel = Program.CreateKernel("DotMul"))
            {
                kernel.SetMemoryArgument(0, aBuffer);
                kernel.SetMemoryArgument(1, bBuffer);
                kernel.SetMemoryArgument(2, rBuffer);
                kernel.SetValueArgument(3, RA);
                kernel.SetValueArgument(4, RB);
                kernel.SetValueArgument(5, C);

                int wi = 1, wj = 1, gi = RA, gj = RB;

                //while(wi<2 && gi % 2 == 0) { wi *= 2; gi /= 2; }
                //while(wj<2 && gj % 2 == 0) { wj *= 2; gj /= 2; }

                kernel.SetValueArgument(6, wi);
                kernel.SetValueArgument(7, wj);

                long li = 1;
                long lj = 1;

                while (li < 8 && gi % 2 == 0) { li *= 2; gi /= 2; }
                while (lj < 8 && gj % 2 == 0) { lj *= 2; gj /= 2; }
                //Debug.WriteLine($"Global {(gi, gj)}; Local {(li, lj)}; RA, RB = {(RA, RB)}");

                //Debug.WriteLine("Created DotMul kernel");
                var startOffset = new[] { 0L, 0L };
                var globalWorkSize = new long[] { RA / wi, RB / wj };
                var localWorkSize = new[] { li, lj };
                //Debug.WriteLine("Starting queue");
                queue.Execute(kernel, startOffset, globalWorkSize, localWorkSize, null);
                queue.Flush();
                queue.Finish();

                GCHandle arrCHandle = GCHandle.Alloc(r, GCHandleType.Pinned);
                queue.Read(rBuffer, true, 0, r.Length, arrCHandle.AddrOfPinnedObject(), null);
                arrCHandle.Free();
            }
        }
       
        private static void Free()
        {
            DotMulKernel.Dispose();            
            Program.Dispose();            
            Context.Dispose();
        }

        private static readonly string DotMulKernelCode = @"
            kernel void DotMul(global const float* a, global const float* b, global float* r, int RA, int RB, int C, int gi, int gj)
            {
                int i0 = get_global_id(0)*gi;
                int j0 = get_global_id(1)*gj;
                for(int i=i0;i<i0+gi;i++)
                    for(int j=j0;j<j0+gj;j++)
                    {
                        float s = 0;
                        for (int c = C-1; c >= 0; c--) s += a[C*i+c] * b[C*j+c];
                        r[j*RA+i]=s;                
                    }                
            }
        ";

        static readonly string KernelCode = DotMulKernelCode;        

        public static void RuntimeTest()
        {
            void TestRandomMul(int ra, int rb, int c)
            {
                float[] x = new float[ra * c];
                float[] y = new float[rb * c];
                var r = new Random();
                for (int i = 0; i < x.Length; i++) x[i] = (float)r.NextDouble();
                for (int i = 0; i < y.Length; i++) y[i] = (float)r.NextDouble();
                float[] r1 = new float[ra * rb];
                float[] r2 = new float[ra * rb];
                Img2Col.DotMul(x, y, r1, ra, rb, c);
                CLBinding.DotMul(x, y, r2, ra, rb, c);

                for (int i = 0; i < ra * rb; i++)
                {
                    if (System.Math.Abs(r1[i] - r2[i]) > 1e-4f) 
                    {
                        Debug.WriteLine($"Failed TestRandomMul({ra},{rb},{c})");
                        Debug.WriteLine(string.Join(", ", r1));
                        Debug.WriteLine(string.Join(", ", r2));
                        throw new InvalidOperationException($"Failed TestRandomMul({ra},{rb},{c})");
                    }
                }
            }
            TestRandomMul(4, 4, 1);
            TestRandomMul(64, 64, 8);
        }


    }
}