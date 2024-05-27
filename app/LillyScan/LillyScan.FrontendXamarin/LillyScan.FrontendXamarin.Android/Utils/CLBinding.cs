using Android.Service.VR;
using Cloo;
using Cloo.Bindings;
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

        public static void Init()
        {
            Debug.WriteLine("Platform");
            Platform = ComputePlatform.Platforms[0];
            Debug.WriteLine("Context");
            Context = new ComputeContext(ComputeDeviceTypes.Gpu, new ComputeContextPropertyList(Platform), null, IntPtr.Zero);
            Debug.WriteLine("Program");
            Program = new ComputeProgram(Context, CalculateKernel);            
            Program.Build(null, null, null, IntPtr.Zero);            
            Debug.WriteLine("Kernel");
            Kernel = Program.CreateKernel("Calc");
            DotMulKernel = Program.CreateKernel("DotMul");
            Debug.WriteLine("Done");
        }
   
        public static void DotMul(float[] a, float[] b, float[] r, int RA, int RB, int C)
        {
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

            //Debug.WriteLine("Created DotMul kernel");
            var startOffset = new[] { 0L, 0L };
            var globalWorkSize = new long[] { RA, RB };
            var localWorkSize = new[] { 1L, 1L };
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
            kernel void DotMul(global const float* a, global const float* b, global float* r, int RA, int RB, int C)
            {
                int i = get_global_id(0);
                int j = get_global_id(1);                
                float s = 0;                
                for (int c = C-1; c >= 0; c--)                    
                    s += a[C*i+c] * b[C*j+c];
                r[j*RA+i]=s;                
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

    }
}