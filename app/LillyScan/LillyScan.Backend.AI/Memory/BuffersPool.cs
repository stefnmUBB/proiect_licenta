using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace LillyScan.Backend.AI.Memory
{
    internal static class BuffersPool
    {
        internal static Dictionary<Buffer, int> Buffers = new Dictionary<Buffer, int>();

        static TextWriter Log = Console.Out;

        internal static unsafe Buffer CreateFromArray(float[] buffer)
        {
            var bufflen = buffer.Length * sizeof(float);
            float* buffptr = (float*)Marshal.AllocHGlobal(bufflen).ToPointer();
            fixed (float* source = &buffer[0])
            {                
                float* iters = source;
                float* iterd = buffptr;
                for (int i = 0; i < buffer.Length; i++) *iterd++ = *iters++;
            }
            var b = new Buffer(buffptr, buffer.Length); 
            Log.WriteLine($"[BP] Created {b}");
            return b;
        }

        internal static unsafe void FreeBuffer(Buffer buffer)
        {
            Log.WriteLine($"[BP] Released {buffer}");
            Marshal.FreeHGlobal(new IntPtr(buffer.Pointer));            
        }

        internal static BufferAccessor AccessBuffer(Buffer buffer, int offset = 0)
        {
            if (!Buffers.ContainsKey(buffer))
                Buffers[buffer] = 1;
            else
                Buffers[buffer]++;
            Log.WriteLine($"[BP] Gained access for {buffer} (now {Buffers[buffer]})");
            return new BufferAccessor(buffer, offset);
        }

        internal static void ReleaseBufferAccess(Buffer buffer)
        {
            if (!Buffers.TryGetValue(buffer, out int count))
                throw new InvalidOperationException("Releasing unregistered buffer");
            if (count <= 0)
                throw new InvalidOperationException("Usage count cannot be 0 or negative");
            Log.WriteLine($"[BP] Released access for {buffer} (remains {count - 1})");
            if (count == 1)
            {
                Buffers.Remove(buffer);
                FreeBuffer(buffer);
            }
            else
            {
                Buffers[buffer]--;
            }
        }
    }
}
