using LillyScan.Backend.Imaging;
using LillyScan.Backend.Math;
using LillyScan.Backend.Parallelization;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace LillyScan.Backend
{
    public abstract class HTREngine : IHTREngine
    {        
        public RawBitmap SelectTiled64(RawBitmap bitmap, bool parallel = false, bool preview=false, CancellationToken? cancellationToken = null)
        {
            var ow = bitmap.Width;
            var oh = bitmap.Height;
            var img = bitmap.Resize(256, 256);
            img = img.AverageChannels(disposeOriginal: true);

            var tiles = img.ToTiles(64, 64);
            img.Dispose();
            cancellationToken?.ThrowIfCancellationRequested();            

            if (parallel)
            {
                Atomic<bool> canceled = new Atomic<bool>(false);
                //Atomic<int> counter = 0;
                Parallel.ForEach(Partitioner.Create(0, tiles.Length, 4), range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++) 
                    {
                        var buffer = tiles[i].ToArray(disposeBitmap: true);                        
                        var segmentedBuffer = Segment64(buffer, preview);
                        var segm = new RawBitmap(64, 64, 1, segmentedBuffer);
                        tiles[i] = segm;
                        //counter.Increment();
                        if (cancellationToken?.IsCancellationRequested ?? false)
                        {
                            canceled.Set(false);
                            break;
                        }                        
                    }
                });
                //Console.WriteLine($"[HTREngine] Counter = {counter}");
                if (canceled.Get())
                {
                    for (int i = 0; i < tiles.Length; i++)
                        tiles[i].Dispose();
                    cancellationToken?.ThrowIfCancellationRequested();
                }
            }
            else
            {
                bool canceled = false;
                for (int i = 0; i < tiles.Length; i++) 
                {
                    var segm = new RawBitmap(64, 64, 1, Segment64(tiles[i].ToArray(), preview));
                    tiles[i].Dispose();
                    tiles[i] = segm;
                    if (cancellationToken?.IsCancellationRequested ?? false)
                    {
                        canceled = true;
                        break;
                    }
                }
                if(canceled)
                {
                    for (int i = 0; i < tiles.Length; i++)
                        tiles[i].Dispose();
                    cancellationToken?.ThrowIfCancellationRequested();
                }
            }

            img = RawBitmaps.FromTiles(tiles, 4, 4);        
            for (int i = 0; i < tiles.Length; i++)
                tiles[i].Dispose();            

            var pred = img.Resize(ow, oh, disposeOriginal: true);
            pred = pred.Threshold(inPlace: true);
            return pred;
        }

        public abstract string Predict(IReadMatrix<double> image);
        public string Predict(ImageRGB image) => Predict(ToGrayscaleDefault(image));
        private static Matrix<double> ToGrayscaleDefault(ImageRGB img)
            => Matrices.DoEachItem(img, x => (x.R.Value + x.G.Value + x.B.Value) / 3);        

        public unsafe virtual byte[] Segment(byte[] image)
        {
            if (image.Length != 256 * 256)
                throw new ArgumentException($"DefaultHTREngine.Segment: Invalid input length");

            var normalized = new float[256 * 256];

            fixed (byte* s = &image[0])
            fixed (float* d = &normalized[0])
            {
                var si = s;
                var di = d;
                for (int i = 0; i < 256 * 256; i++)
                    *di++ = *si++ / 255.0f;
            }

            var predicted = Segment(normalized);
            var result = new byte[256 * 256 * 3];

            fixed (float* s = &predicted[0]) 
            fixed (byte* d = &result[0])
            {
                var si = s;
                var di = d;
                for (int i = 0; i < 256 * 256; i++)
                    *di++ = (byte)((*si++).Clamp(0, 1) * 255);
            }
            return result;
        }

        public abstract float[] Segment(float[] image);

        public abstract float[] Segment64(float[] image, bool preview = false);        
    }
}
