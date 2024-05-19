using LillyScan.Backend.AI.Models;
using LillyScan.Backend.Imaging;
using LillyScan.Backend.Math;
using LillyScan.Backend.Parallelization;
using LillyScan.Backend.Properties;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LillyScan.Backend.HTR
{
    public class BuiltInHTREngine : IHTREngine
    {
        private readonly Model NormalSegmentationModel;
        private readonly Model LinearSegmentationModel;
        private readonly Model PreviewSegmentationModel;
        private readonly Model PaddedLinearSegmentationModel;
        private readonly Model RecognitionModelEn;

        public BuiltInHTREngine()
        {
            NormalSegmentationModel = ModelLoader.LoadFromBytes(Resources.SegmentationModel64_lsm);
            LinearSegmentationModel = ModelLoader.LoadFromBytes(Resources.LinearSegmentationModel64_lsm);
            PreviewSegmentationModel = ModelLoader.LoadFromBytes(Resources.PreviewSegmentationModel64_lsm);
            PaddedLinearSegmentationModel = ModelLoader.LoadFromBytes(Resources.PaddedLinearSegmentationModel64_lsm);
            RecognitionModelEn = ModelLoader.LoadFromBytes(Resources.RecognitionModelEn_lsm);
        }

        static int k = 0;
        public unsafe string PredictTextLine(RawBitmap bitmap, ProgressMonitor progressMonitor = null, string taskName = null)
        {
            progressMonitor?.PushTask(taskName ?? "PredictTextLine", 1);

            using (var tmp = new RawBitmap(1024, 64, 1)) 
            {
                var bmp = bitmap.AverageChannels();
                bmp.CheckNaN();
                if (bmp.Width > 1024 || bmp.Height > 64)
                {
                    var scale = System.Math.Min(64f / bmp.Height, 1024f / bmp.Width);
                    bmp = bmp.Resize((int)(bmp.Width * scale), (int)(bmp.Height * scale));
                    bmp.CheckNaN();
                }
                tmp.Clear();
                tmp.DrawImage(bmp, System.Math.Min(16, 1024 - bmp.Width), (64 - bmp.Height) / 2, inPlace: true);
                tmp.CheckNaN();
                bmp.Dispose();

                CCAction?.Invoke(tmp, "");

                var inputBuffer = new float[64 * 1024];
                for (int i = 0; i < inputBuffer.Length; i++) inputBuffer[i] = tmp.Buffer[i];
                var input = new Tensor<float>(new Shape(1, 64, 1024, 1), inputBuffer);
                var predicted = RecognitionModelEn.Call(new[] { input }, progressMonitor: progressMonitor);
                var buffer = predicted[0].Buffer;

                string text = "";
                for (int i=0;i<128;i++)
                {
                    var seq = buffer.Skip(82 * i).Take(82).ToArray();
                    int am = seq.ArgMax();
                    if (am < CharactersEn.Length)
                        text += CharactersEn[am];
                }

                /*using (var f = File.OpenWrite($"cc\\Text{k++}.txt"))
                using (var w = new StreamWriter(f))
                {
                    w.WriteLine("[::]" + text + "[::]");
                    predicted[0].Print("Pred", w);
                }*/
                progressMonitor?.PopTask();
                return text;
            }            
        }

        public RawBitmap SegmentTiles64(RawBitmap bitmap, SegmentationType segmentationType, bool parallel = true,
            bool resizeToOriginal=true,
            ProgressMonitor progressMonitor = null, string taskName = null)
        {
            if(parallel && progressMonitor!=null)
            {
                progressMonitor = new ProgressMonitor(progressMonitor.CancellationToken);                
                Debug.WriteLine("Progress monitor is not available in parallel mode. It has been disabled.");
            }
            var originalWidth = bitmap.Width;
            var originalHeight = bitmap.Height;

            progressMonitor?.PushTask(taskName ?? "SegmentTiles64", 16);

            Model segmentationModel = null;

            RawBitmap img;

            if ((segmentationType & SegmentationType.Padded) == 0)
            {                
                switch (segmentationType)
                {
                    case SegmentationType.Linear: segmentationModel = LinearSegmentationModel; break;
                    case SegmentationType.Normal: segmentationModel = NormalSegmentationModel; break;
                    case SegmentationType.Preview: segmentationModel = PreviewSegmentationModel; break;
                }
                img = SegmentTiles64Unpadded(bitmap, segmentationModel, parallel, progressMonitor);
            }
            else
            {
                switch (segmentationType)
                {
                    case SegmentationType.PaddedLinear: segmentationModel = PaddedLinearSegmentationModel; break;
                    default: throw new NotImplementedException(segmentationType.ToString());                    
                }
                img = SegmentTiles64Padded(bitmap, segmentationModel, parallel, progressMonitor);
            }

            if (resizeToOriginal)
                img = img.Resize(originalWidth, originalHeight, disposeOriginal: true);
            img = img.Threshold(inPlace: true);

            progressMonitor?.PopTask();
            return img;
        }

        public Action<RawBitmap, string> CCAction = null;

        public unsafe LocalizedMask[] SegmentLines(RawBitmap bitmap, ProgressMonitor progressMonitor = null, string taskName = null)
        {
            progressMonitor?.PushTask(taskName ?? nameof(SegmentLines), 4);

            var normalSegm = SegmentTiles64(bitmap, SegmentationType.Normal, parallel: false, resizeToOriginal: false, progressMonitor, "NormalSegm");
            progressMonitor?.AdvanceOneStep();
            var linearSegm = SegmentTiles64(bitmap, SegmentationType.PaddedLinear, parallel: false, resizeToOriginal: false, progressMonitor, "LinearSegm");
            progressMonitor?.AdvanceOneStep();

            linearSegm = linearSegm.Threshold(0.5f, inPlace: true);
            CCAction?.Invoke(linearSegm, "");

            var blurred = linearSegm.Filter3x3(new[] { 1f / 9, 1f / 9, 1f / 9, 1f / 9, 1f / 9, 1f / 9, 1f / 9, 1f / 9, 1f / 9, });
            blurred = blurred.Threshold(0.5f, inPlace: true);
            
            var cc = ConnectedComponents.FindInRawBitmapBinaryMask(linearSegm, progressMonitor);
            progressMonitor?.AdvanceOneStep();
            var masks = cc.Components.Select(_ => _.ToLocalizedMask()).ToArray();            
            var lines = LineDefragmentation.BuildSubsets(masks.Where(m => m.Area > 10).ToArray(), 20, CCAction);            
            var linesMask = LineDefragmentation.BuildLinesMask(lines, CCAction);                        

            normalSegm = normalSegm.Threshold(inPlace: true);
            CCAction?.Invoke(normalSegm, "ns");            
            var normalMasks = ConnectedComponents.FindInRawBitmapBinaryMask(normalSegm, progressMonitor)
                .Components.Select(_ => _.ToLocalizedMask())
                .Where(_ => _.Area > 10).ToArray();
            progressMonitor?.AdvanceOneStep();

            normalSegm.Clear();
            CCAction?.Invoke(normalSegm, "ns");
            foreach (var m in normalMasks)
            {
                Console.WriteLine(m);
                LineDefragmentation.DrawMask(normalSegm, m, 1, 1, 1);
            }
            CCAction?.Invoke(normalSegm, "ns");

            LineDefragmentation.AndMask(linesMask, normalSegm);
            LineDefragmentation.MaskView(linesMask, CCAction);

            var linesLocalized = new List<LocalizedMask>();
            float scaleX = bitmap.Width / 256f;
            float scaleY = bitmap.Height / 256f;            

            for (int i = 0; i < lines.Length; i++)
            {
                var points = linesMask.Select((x, ix) => (x, ix)).Where(_ => _.x == i + 1).Select(_ => (_.ix % 256, _.ix / 256)).ToArray();
                Console.WriteLine(points.ToArray());
                var mask = new LocalizedMask(points);
                mask.ComputeMetadata();
                SmoothFillMask(mask);
                if (mask.Area == 0) continue;
                mask = mask.Rescale(scaleX, scaleY);
                mask.ComputeMetadata();                

                linesLocalized.Add(mask);                
            }


            /*foreach (var lm in linesLocalized)
            {
                using (var bmp = new RawBitmap(bitmap.Width, bitmap.Height, 3)) 
                {
                    LineDefragmentation.DrawMask(bmp, lm);
                    CCAction(bmp, "");
                }
            }*/
            Debug.WriteLine("HERE?????????????????????????????");
            progressMonitor?.PopTask();
            Debug.WriteLine("HERE2?????????????????????????????");
            return linesLocalized.ToArray();
        }

        private void SmoothFillMask(LocalizedMask mask)
        {
            var dy = new int[] { -1, 0, 1, 0 };
            var dx = new int[] { 0, -1, 0, 1 };
            var contour = new List<(int X, int Y)>();

            for(int y=0;y<mask.Height;y++)
            {
                for(int x=0;x<mask.Width;x++)
                {
                    if (mask[y,x]!=0)
                    {
                        if(x==0 || y==0 || x==mask.Width-1 || y==mask.Height-1)
                        {
                            contour.Add((x, y));
                        }
                        else
                        {
                            int n0 = 0;
                            for (int i = 0; i < 4; i++)
                            {
                                int iy = y + dy[i], ix = x + dx[i];
                                if (mask[iy, ix] == 0) n0++;                                
                            }
                            if(n0>0)
                            {
                                contour.Add((x, y));
                            }
                        }                                                
                    }
                }
            }

            if (contour.Count == 0) return;
            var gx = (int)contour.Average(_ => _.X);
            var gy = (int)contour.Average(_ => _.Y);

            var T = (int)System.Math.Sqrt(mask.Width * mask.Width + mask.Height * mask.Height);

            for(int i=0;i<contour.Count;i++)
            {
                int y0 = contour[i].Y, x0 = contour[i].X;
                for (int t = 0; t < T; t++)
                {
                    var iy = y0 + (gy - y0) * t / T;
                    var ix = x0 + (gx - x0) * t / T;                    
                    mask[iy, ix] = 1;
                }
            }

        }
        static readonly string CharactersEn = " !\"#%&'()*+,-./0123456789:;>?ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        #region Private Methods

        private static RawBitmap SegmentTiles64Unpadded(RawBitmap bitmap, Model segmentationModel, bool parallel, ProgressMonitor progressMonitor)
        {            
            var img = bitmap.Resize(256, 256);
            if (img.Channels > 1)
                img = img.AverageChannels(disposeOriginal: true);
            var tiles = img.ToTiles(64, 64, disposeOriginal: true);
            var opstatus = parallel
                ? ProcessTilesSegmentationInPlaceParallel(tiles, segmentationModel, progressMonitor)
                : ProcessTilesSegmentationInPlaceSequencial(tiles, segmentationModel, progressMonitor);
            if (opstatus == false)
            {
                for (int i = 0; i < tiles.Length; i++)
                    tiles[i].Dispose();                
                progressMonitor?.CancellationToken?.ThrowIfCancellationRequested();
                return null;
            }

            img = RawBitmaps.FromTiles(tiles, 4, 4, disposeOriginal: true);            
            return img;
        }

        private static RawBitmap SegmentTiles64Padded(RawBitmap bitmap, Model segmentationModel, bool parallel, ProgressMonitor progressMonitor)
        {
            var img = bitmap.Resize(256, 256);
            if (img.Channels > 1)
                img = img.AverageChannels(disposeOriginal: true);
            var tiles = img.ToTilesPadded(64, 64, padding: 8, disposeOriginal: true);
            var opstatus = parallel
                ? ProcessTilesSegmentation80_64Parallel(tiles, segmentationModel, progressMonitor)
                : ProcessTilesSegmentation80_64Sequencial(tiles, segmentationModel, progressMonitor);
            if (opstatus == false)
            {
                for (int i = 0; i < tiles.Length; i++)
                    tiles[i].Dispose();                
                progressMonitor?.CancellationToken?.ThrowIfCancellationRequested();
                return null;
            }

            img = RawBitmaps.FromTiles(tiles, 4, 4, disposeOriginal: true);            
            return img;
        }    

        private static bool ProcessTilesSegmentationInPlaceParallel(RawBitmap[] tiles, Model segmentationModel, ProgressMonitor progressMonitor = null)
        {
            Atomic<bool> canceled = new Atomic<bool>(false);            
            Parallel.ForEach(Partitioner.Create(0, tiles.Length, 4), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    SegmentTile64(segmentationModel, tiles[i], progressMonitor);
                    progressMonitor?.AdvanceOneStep(throwIfCancelled: false);
                    //Debug.WriteLine($"AdvancedOne");
                    if (progressMonitor?.TaskCanceled ?? false) 
                    {
                        canceled.Set(false);
                        break;
                    }
                }
            });
            return !canceled.Get();         
        }
        private static bool ProcessTilesSegmentationInPlaceSequencial(RawBitmap[] tiles, Model segmentationModel, ProgressMonitor progressMonitor = null)
        {
            Console.WriteLine("ProcessTilesSegmentationInPlaceSequencial");
            bool canceled = false;
            for (int i = 0; i < tiles.Length; i++)
            {
                SegmentTile64(segmentationModel, tiles[i], progressMonitor);                                
                progressMonitor?.AdvanceOneStep(throwIfCancelled: false);
                Debug.WriteLine($"AdvancedOne");
                if (progressMonitor?.TaskCanceled ?? false)
                {
                    canceled = true;
                    break;
                }                
            }
            return !canceled;
        }

        private static bool ProcessTilesSegmentation80_64Parallel(RawBitmap[] tiles, Model segmentationModel, ProgressMonitor progressMonitor = null)
        {
            Atomic<bool> canceled = new Atomic<bool>(false);
            Parallel.ForEach(Partitioner.Create(0, tiles.Length, 4), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    SegmentTile80_64(segmentationModel, ref tiles[i], progressMonitor);                    
                    progressMonitor?.AdvanceOneStep(throwIfCancelled: false);
                    Debug.WriteLine($"AdvancedOne");
                    if (progressMonitor?.TaskCanceled ?? false)
                    {
                        canceled.Set(false);
                        break;
                    }
                }
            });
            return !canceled.Get();
        }

        private static bool ProcessTilesSegmentation80_64Sequencial(RawBitmap[] tiles, Model segmentationModel, ProgressMonitor progressMonitor=null)
        {
            Console.WriteLine("ProcessTilesSegmentation80_64Sequencial");
            bool canceled = false;
            for (int i = 0; i < tiles.Length; i++)
            {
                SegmentTile80_64(segmentationModel, ref tiles[i], progressMonitor);                
                progressMonitor?.AdvanceOneStep(throwIfCancelled: false);
                Debug.WriteLine($"AdvancedOne");
                if (progressMonitor?.TaskCanceled ?? false)
                {
                    canceled = true;
                    break;
                }
            }
            return !canceled;
        }

        private static void SegmentTile64(Model segmentationModel, RawBitmap tile, ProgressMonitor progressMonitor)
        {
            var input = new[] { new Math.Tensor<float>((1, 64, 64, 1), tile.ToArray()) };
            try
            {
                var result = segmentationModel.Call(input, progressMonitor: progressMonitor, verbose: false)[0].Buffer.Buffer;                
                tile.InplaceCopyFrom(result);
            }
            catch(OperationCanceledException e)
            {

            }
        }

        private static void SegmentTile80_64(Model segmentationModel, ref RawBitmap tile, ProgressMonitor progressMonitor)
        {
            var input = new[] { new Math.Tensor<float>((1, 80, 80, 1), tile.ToArray()) };
            try
            {
                var result = segmentationModel.Call(input, progressMonitor: progressMonitor, verbose: false)[0].Buffer.Buffer;                
                tile.Dispose();
                tile = new RawBitmap(64, 64, 1, result);
            }
            catch(OperationCanceledException e)
            {

            }
        }


        #endregion
    }
}
