using LillyScan.Backend.Utils;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LillyScan.Backend.Imaging
{
    public unsafe class RawBitmap : IDisposable
    {
        private float* pBuffer;
        public float* Buffer 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => pBuffer;
        }
        public readonly int Width;
        public readonly int Height;
        public readonly int Channels;
        public readonly int Stride;

        public RawBitmap(RawBitmap bmp) : this(bmp.Width, bmp.Height, bmp.Channels)
        {
            for (int i = 0; i < Stride * Height; i++)
                pBuffer[i] = bmp.pBuffer[i];
        }

        public RawBitmap(int width, int height, int channels, float[] data) : this(width, height, channels)
        {
            for (int i = 0; i < Stride * Height; i++)
                pBuffer[i] = data[i];
        }

        public unsafe RawBitmap(int width, int height, int channels, float* data) : this(width, height, channels)
        {
            for (int i = 0; i < Stride * Height; i++)
                pBuffer[i] = data[i];
        }

        public RawBitmap(int width, int height, int channels)
        {
            Width = width;
            Height = height;
            Channels = channels;
            Stride = Width * Channels;
            pBuffer = (float*)Marshal.AllocHGlobal(Height * Stride * sizeof(float)).ToPointer();
            pBuffer[Width * Height * Channels-1] = 1;
            //Console.WriteLine(new IntPtr(pBuffer).ToString());
        }

        public float this[int index] { get => pBuffer[index]; set => pBuffer[index] = value; }
        public float this[int y, int x, int c = 0] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => pBuffer[y * Stride + x * Channels + c];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Buffer[y * Stride + x * Channels + c]=value;
        }        

        public void Dispose()
        {
#if DEBUG
            if (pBuffer == (float*)IntPtr.Zero)
                throw new InvalidOperationException("Attempting to free already disposed memory");
            Marshal.FreeHGlobal(new IntPtr(pBuffer));
            //Debug.WriteLine($"Disposed {Width}x{Height}x{Channels}: {new IntPtr(pBuffer)}");
            pBuffer = (float*)IntPtr.Zero;
#elif RELEASE
            Marshal.FreeHGlobal(new IntPtr(pBuffer));
#endif
        }

        public unsafe float[] ToArray(bool disposeBitmap = false)
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
