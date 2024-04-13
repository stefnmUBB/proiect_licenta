using LillyScan.Backend.Utils;
using System;
using System.Runtime.CompilerServices;
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

        public RawBitmap(RawBitmap bmp) : this(bmp.Width, bmp.Height, bmp.Channels)
        {
            for (int i = 0; i < Stride * Height; i++)
                Buffer[i] = bmp.Buffer[i];
        }

        public RawBitmap(int width, int height, int channels, float[] data) : this(width, height, channels)
        {
            for (int i = 0; i < Stride * Height; i++)
                Buffer[i] = data[i];
        }

        public unsafe RawBitmap(int width, int height, int channels, float* data) : this(width, height, channels)
        {
            for (int i = 0; i < Stride * Height; i++)
                Buffer[i] = data[i];
        }

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
        public float this[int y, int x, int c = 0] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Buffer[y * Stride + x * Channels + c];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal set => Buffer[y * Stride + x * Channels + c]=value;
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(new IntPtr(Buffer));
        }

        public unsafe float[] ToArray(bool disposeBitmap = true)
        {
            var result = new float[Stride*Height];
            float* s = Buffer;
            for (int i = 0; i < result.Length; i++) result[i] = *s++;
            if (disposeBitmap)
                Dispose();
            return result;
        }
    }
}
