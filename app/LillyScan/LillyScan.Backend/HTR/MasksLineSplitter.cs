using LillyScan.Backend.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LillyScan.Backend.Utils;
using System.IO;
using System.Diagnostics;

namespace LillyScan.Backend.HTR
{
    public static class MasksLineSplitter
    {
        private const int DiagLength = 2 * 363;
        static int w = 0;

        public class LinesSplit
        {
            public readonly (float A, float B, float C) LineFit;
            public readonly LocalizedMask[] Masks;
            public readonly float[] ChangedBaseMaskY;
            public readonly float CenterBaseY;
            public readonly int TotalArea;
            public readonly (float X, float Y) BaseVector;
            public readonly (float X, float Y, float Width, float Height) BaseBoundingBox;
            public readonly (float X0, float Y0, float X1, float Y1, float X2, float Y2, float X3, float Y4) BoundingPoly;

            public LinesSplit((float A, float B, float C) lineFit, LocalizedMask[] masks, (float X, float Y) baseVector)
            {
                Console.WriteLine($"ML = {masks.Length}");
                LineFit = lineFit;
                Masks = masks;
                BaseVector = baseVector;
                TotalArea = masks.Sum(_ => _.Area);
                ChangedBaseMaskY = masks.Select(_ => ChangeBase(_.CenterX, _.CenterY, baseVector).Y).ToArray();
                if (ChangedBaseMaskY.Length > 0)
                    CenterBaseY = ChangedBaseMaskY.Average();
                else
                    CenterBaseY = 0;
                var points = masks.SelectMany(_ => _.EnumeratePixels())
                    .Select(_ => ChangeBase(_.X, _.Y, baseVector)).ToArray();

                (var x0, var x1) = points.MinAndMax(_ => _.X);
                (var y0, var y1) = points.MinAndMax(_ => _.Y);
                BaseBoundingBox = (x0, y0, x1 - x0, y1 - y0);

                (var cx0, var cy0) = ChangeBase(x0, y0, BaseVector);
                (var cx1, var cy1) = ChangeBase(x1, y0, BaseVector);
                (var cx2, var cy2) = ChangeBase(x1, y1, BaseVector);
                (var cx3, var cy3) = ChangeBase(x0, y1, BaseVector);
                BoundingPoly = (cx0, cy0, cx1, cy1, cx2, cy2, cx3, cy3);
            }

            public float YDistance(float x, float y)
            {
                var targetY = ChangeBase(x, y, BaseVector).Y;
                return ChangedBaseMaskY.Min(_ => System.Math.Abs(targetY - _));
            }

            public float YDistance(float targetY) => ChangedBaseMaskY.Min(_ => System.Math.Abs(targetY - _));            
        }

        public static LinesSplit[] FindLines(LocalizedMask[] masks, (float X, float Y) baseVector)
        {
            Debug.WriteLine("CummulateY");
            var ycum = CummulateY(masks, baseVector);
            //File.WriteAllText($"dbg_{w++}.txt", $"y=[{ycum.JoinToString(", ")}]");
            Debug.WriteLine("Conv");
            Conv(ycum, 3, 4, 3);
            Conv(ycum, 2, 6, 2);
            Conv(ycum, 1, 8, 1);
            //File.WriteAllText($"dbg_{w++}.txt", $"v=[{ycum.JoinToString(", ")}]");
            Debug.WriteLine("MaskSpikesAndPlateaus");
            var c = MaskSpikesAndPlateaus(ycum);
            //File.WriteAllText($"dbg_{w++}.txt", "p=[" + c.JoinToString(", ") + "]");
            Debug.WriteLine("FindContinuousActivations");
            var lineZones = FindContinuousActivations(c).ToArray();
            int linesCount = lineZones.Length;

            //File.WriteAllText($"dbg_{w++}.txt", lineZones.JoinToString("\n"));

            Debug.WriteLine("ArgMin");
            var maskLine = new int[masks.Length];
            for(int i=0;i<masks.Length;i++)
            {
                var mask = masks[i];
                maskLine[i] = lineZones
                    .Select(l => System.Math.Abs(l.Y- DiagLength / 2 + l.Length / 2 - ChangeBase(mask.CenterX, mask.CenterY, baseVector).Y))
                    .ArgMin();
            }

            var result = new LinesSplit[linesCount];
            var maskIndices = Enumerable.Range(0, masks.Length).ToArray();

            for (int i=0;i<linesCount;i++)
            {
                var lineFit = (baseVector.X, baseVector.Y, lineZones[i].Y + lineZones[i].Length * 0.5f);
                var lineMasks = maskIndices.Where(k => maskLine[k] == i).Select(k => masks[k]).ToArray();
                result[i] = lineMasks.Length > 0 ? new LinesSplit(lineFit, lineMasks, baseVector) : null;
            }
            return result.Where(_ => _ != null).ToArray();
        }

        private static IEnumerable<(int Y, int Length)> FindContinuousActivations(int[] c)
        {
            int y = -1, l = 0;
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i]==0 && l>0)
                {
                    yield return (y, l);
                    (y, l) = (-1, 0);                    
                    continue;
                }
                if (c[i] > 0) 
                {
                    if (y < 0) y = i;
                    l++;
                }
            }
            if (l > 0)
                yield return (y, l);
        }

        private static int[] MaskSpikesAndPlateaus(float[] ycum)
        {
            int L = ycum.Length;
            var c = new int[L];
            if (L >= 2)
            {
                if(ycum[0] > ycum[1]) c[0] = 1;
                if (ycum[L - 1] > ycum[L - 2]) c[L - 1] = 1;
            }            
            for (int i = 1; i < L - 1; i++) 
            {
                if (ycum[i] > ycum[i - 1] || ycum[i] > ycum[i + 1])
                    c[i] = 1;
            }

            for (int i = 1; i < L; i++) 
            {
                if (c[i - 1] > 0 && System.Math.Abs(ycum[i] - ycum[i - 1]) < 0.001f) 
                    c[i] = 1;
            }

            return c;
        }

        private static unsafe float[] CummulateY(LocalizedMask[] masks, (float X, float Y) baseVector)
        {
            var ycum = new float[DiagLength];
            foreach (var mask in masks)
            {
                Debug.WriteLine($"{(mask.X, mask.Y)} {(mask.Width, mask.Height)}, {baseVector}");
                for (int y = 0; y < mask.Height; y++)
                {
                    for (int x = 0; x < mask.Width; x++)
                    {
                        if (mask.Data[y * mask.Width + x] == 0) continue;
                        var ny = ChangeBase(mask.X + x, mask.Y + y, baseVector).Y;
                        //Debug.WriteLine($"{DiagLength / 2 + (int)ny} / {DiagLength}");
                        ycum[DiagLength / 2 + (int)ny]++;
                    }
                }
            }
            return ycum;
        }
        public static void Conv(float[] ycum, float a, float b, float c)
        {
            if (ycum.Length <= 1) return;
            float s = a + b + c;
            (a, b, c) = (a / s, b / s, c / s);
            int L = ycum.Length;
            var tmp = new float[L];
            tmp[0] = ycum[0];
            tmp[L - 1] = ycum[L - 1];
            for (int i = 1; i < L - 1; i++) tmp[i] = a * ycum[i - 1] + b * ycum[i] + c * ycum[i + 1];
            Array.Copy(tmp, ycum, tmp.Length);
        }

        public static (float X, float Y) ChangeBase(float x, float y, (float X, float Y) baseVector)
        {
            return (y * baseVector.X - x * baseVector.Y, x * baseVector.X + y * baseVector.Y);
        }       
    }
}
