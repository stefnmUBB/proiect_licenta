using Android.Service.VR;
using AndroidX.ConstraintLayout.Core.Motion.Utils;
using Cloo;
using Cloo.Bindings;
using LillyScan.Backend.Math;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LillyScan.FrontendXamarin.Droid.Utils
{
    public static class CLBinding
    {
        private static ComputePlatform Platform;
        private static ComputeContext Context;
        private static ComputeProgram Program;
        private static ComputeKernel Kernel;
        private static ComputeKernel DotMulKernel;

        private static string ProgramBuildFlags = "-cl-fast-relaxed-math -cl-no-signed-zeros";
        public static void Init()
        {
            Debug.WriteLine("Platform");
            Platform = ComputePlatform.Platforms[0];
            Debug.WriteLine("Context");
            Context = new ComputeContext(ComputeDeviceTypes.Gpu, new ComputeContextPropertyList(Platform), null, IntPtr.Zero);
            Debug.WriteLine("Program");
            Program = new ComputeProgram(Context, CalculateKernel);            
            Program.Build(null, ProgramBuildFlags, null, IntPtr.Zero);            
            Debug.WriteLine("Kernel");
            Kernel = Program.CreateKernel("Calc");
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
            using var aBuffer = new ComputeBuffer<float>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, a);
            using var bBuffer = new ComputeBuffer<float>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, b);
            using var rBuffer = new ComputeBuffer<float>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, r);
            using var queue = new ComputeCommandQueue(Context, Context.Devices[0], ComputeCommandQueueFlags.None);
            using var kernel = Program.CreateKernel("DotMul");
            kernel.SetMemoryArgument(0, aBuffer);
            kernel.SetMemoryArgument(1, bBuffer);
            kernel.SetMemoryArgument(2, rBuffer);
            kernel.SetValueArgument(3, RA);
            kernel.SetValueArgument(4, RB);
            kernel.SetValueArgument(5, C);

            int wi = 1, wj = 1, gi=RA, gj=RB;

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

        public static void Run()
        {
            int[] r1 = new int[] {8, 2, 3, 4};
            int[] r2 = new int[] {4, 3, 2, 5};
            int[] r3 = new int[4];
            int rowSize = r1.Length;            
            
            ComputeBuffer<int> row1Buffer = new ComputeBuffer<int>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, r1);            
            ComputeBuffer<int> row2Buffer = new ComputeBuffer<int>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, r2);            
            ComputeBuffer<int> resultBuffer = new ComputeBuffer<int>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, new int[4]);
            
            var queue = new ComputeCommandQueue(Context, Context.Devices[0], ComputeCommandQueueFlags.None);

            Kernel.SetMemoryArgument(0, row1Buffer); // set the integer array
            Kernel.SetMemoryArgument(1, row2Buffer); // set the integer array
            Kernel.SetValueArgument(2, rowSize); // set the array size
            Kernel.SetMemoryArgument(3, resultBuffer); // set the integer array            
            queue.Execute(Kernel, new[] { 0L }, new[] { 4L }, new[] { 1L }, null);
            //queue.ExecuteTask(Kernel, null);
            queue.Finish();

            GCHandle arrCHandle = GCHandle.Alloc(r3, GCHandleType.Pinned);
            queue.Read<int>(resultBuffer, true, 0, r3.Length, arrCHandle.AddrOfPinnedObject(), null);

            Console.WriteLine("display result from gpu buffer:");
            for (int i = 0; i < r3.Length; i++)
                Console.WriteLine(r3[i]);

            arrCHandle.Free();
            row1Buffer.Dispose();
            row2Buffer.Dispose();

            queue.Dispose();

        }

        private static void Free()
        {
            DotMulKernel.Dispose();
            Kernel.Dispose();
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

        static string CalculateKernel
        {
            get
            {
                // you could put your matrix algorithm here an take the result in array m3
                return @"
            kernel void Calc(global int* m1, global int* m2, int size, global int* m3) 
            {
                int i=get_global_id(0);                
                int val = m2[i];
                printf("" %d / %d\n"",m1[i],m2[i] );
                m3[i] = val * 4;                
            }
            " + DotMulKernelCode;
            }
        }      

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