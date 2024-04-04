using LillyScan.Backend.Utils;
using System;
using System.Runtime.InteropServices;

namespace LillyScan.Backend.Imaging
{
    public unsafe class RawBitmap : IDisposable
    {        
        public readonly float* Buffer;
        public readonly int Width;
        public readonly int Height;
        public readonly int Channels;
        public readonly int Stride;        

        public RawBitmap(int width, int height, int channels)
        {
            Width = width;
            Height = height;
            Channels = channels;
            Stride = Width * Channels;
            Buffer = (float*)Marshal.AllocHGlobal(Height * Stride * sizeof(float)).ToPointer();
            Buffer[Width * Height * Channels-1] = 1;
            Console.WriteLine(new IntPtr(Buffer).ToString());
        }

        public float this[int index] { get => Buffer[index]; set => Buffer[index] = value; }
        public float this[int y, int x, int c=0] => Buffer[y*Stride+x*Channels+c];        

        public void Dispose()
        {
            Marshal.FreeHGlobal(new IntPtr(Buffer));
        }

        public unsafe float[] ToArray()
        {
            var result = new float[Stride*Height];
            float* s = Buffer;
            for (int i = 0; i < result.Length; i++) result[i] = *s++;
            return result;
        }
    }
}
