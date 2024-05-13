using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.Imaging
{
    public class ConnectedComponents
    {
        public readonly struct ComponentData
        {
            public readonly int Id;
            public readonly int Area;
            public readonly (int Y, int X) Centroid;
            public readonly (double B0, double B1) RegressionLine;
            public readonly (int Y, int X)[] Points;

            public ComponentData(int id, int area, (int Y, int X) centroid, (double B0, double B1) regressionLine, (int, int)[] points)
            {
                Id = id;
                Area = area;
                Centroid = centroid;
                RegressionLine = regressionLine;
                Points = points;
            }

            public LocalizedMask ToLocalizedMask()
            {
                int minX = Points[0].X, minY = Points[0].Y;
                int maxX = Points[0].X, maxY = Points[0].Y;

                for (int i = 1; i < Points.Length; i++)
                {
                    if (Points[i].X < minX) minX = Points[i].X;
                    if (Points[i].X > maxX) maxX = Points[i].X;
                    if (Points[i].Y < minY) minY = Points[i].Y;
                    if (Points[i].Y > maxY) maxY = Points[i].Y;
                }

                var width = maxX - minX + 1;
                var height = maxY - minY + 1;

                var buffer = new byte[height * width];
                for (int i = 0; i < Points.Length; i++)
                    buffer[(Points[i].Y - minY) * width + Points[i].X - minX] = 1;
                var mask = new LocalizedMask(minX, minY, width, height, buffer);
                mask.ComputeMetadata();
                return mask;
            }
        }

        public readonly int[] Map;
        public ComponentData[] Components { get; private set; }

        public readonly int Height;
        public readonly int Width;

        public ConnectedComponents(int[] map, ComponentData[] components, int height, int width)
        {
            Map = map;
            Components = components;
            Height = height;
            Width = width;
        }      


        private static readonly int[] dy = new[] { -1, 0, 1, 0 };
        private static readonly int[] dx = new[] { 0, -1, 0, 1 };
        private static unsafe ComponentData FindOneComponentInRawBitmapBinaryMask(RawBitmap bmp, int* map, int y0, int x0, int id)
        {
            int area = 0;
            var queue = new Queue<(int, int)>();
            HashSet<(int Y, int X)> visited = new HashSet<(int, int)>();
            queue.Enqueue((y0, x0));
            visited.Add((y0, x0));
            while (queue.Count>0)
            {
                (int y, int x) = queue.Dequeue();
                //Console.WriteLine($"Dequeued {(y, x)} / {queue.Count}");
                if (map[bmp.Width * y + x] == 0)
                {
                    map[bmp.Width * y + x] = id;
                    area++;
                }

                for(int i=0;i<4;i++)
                {
                    int ix = x + dx[i], iy = y + dy[i];
                    if (ix < 0 || iy < 0 || ix >= bmp.Width || iy >= bmp.Height)
                        continue;
                    var pos = iy * bmp.Width + ix;
                    if (map[pos] != 0 || bmp[pos] == 0) 
                        continue;
                    if (visited.Contains((iy, ix)))
                        continue;
                    queue.Enqueue((iy, ix));
                    visited.Add((iy, ix));
                    //Console.WriteLine($"Enqueued {(iy, ix)} / {queue.Count}");
                }
            }

            var allPoints = visited.ToArray();
            int sy = 0, sx = 0;
            for(int i=0;i<allPoints.Length;i++)
            {
                sy += allPoints[i].Y;
                sx += allPoints[i].X;
            }

            return new ComponentData(id, area, (sy / allPoints.Length, sx / allPoints.Length), (0, 0), allPoints);
        }

        public static unsafe ConnectedComponents FindInRawBitmapBinaryMask(RawBitmap bmp, ProgressMonitor progressMonitor = null, string taskName = null)        
        {
            if (bmp.Channels != 1)
                throw new ArgumentException("Cannot find connected components in multichannel bitmap");

            progressMonitor?.PushTask(taskName ?? "ConComp", bmp.Height);

            var map = new int[bmp.Height * bmp.Width];

            var components = new List<ComponentData>();

            fixed(int* pmap = &map[0])
            {
                for(int y=0;y<bmp.Height;y++)
                {                    
                    for(int x=0;x<bmp.Width;x++)
                    {
                        var pos = y * bmp.Width + x;
                        if (pmap[pos] != 0 || bmp[pos] == 0) 
                            continue;
                        components.Add(FindOneComponentInRawBitmapBinaryMask(bmp, pmap, y, x, components.Count + 1));
                    }
                    progressMonitor?.AdvanceOneStep();
                }
            }
            progressMonitor?.PopTask();
            return new ConnectedComponents(map, components.ToArray(), bmp.Height, bmp.Width);
        }

        private static unsafe ComponentData FindOneComponentInMatrix(int[] mat, int rows, int cols, int* map, int y0, int x0, int id)
        {
            int area = 0;
            var queue = new Queue<(int, int)>();
            HashSet<(int Y, int X)> visited = new HashSet<(int, int)>();
            queue.Enqueue((y0, x0));
            visited.Add((y0, x0));
            int value = mat[y0 * cols + x0];
            while (queue.Count > 0)
            {
                (int y, int x) = queue.Dequeue();
                //Console.WriteLine($"Dequeued {(y, x)} / {queue.Count}");
                if (map[cols * y + x] == 0)
                {
                    map[cols * y + x] = id;
                    area++;
                }

                for (int i = 0; i < 4; i++)
                {
                    int ix = x + dx[i], iy = y + dy[i];
                    if (ix < 0 || iy < 0 || ix >= cols || iy >= rows) 
                        continue;
                    var pos = iy * cols + ix;
                    if (map[pos] != 0 || mat[pos] != value) 
                        continue;
                    if (visited.Contains((iy, ix)))
                        continue;
                    queue.Enqueue((iy, ix));
                    visited.Add((iy, ix));
                    //Console.WriteLine($"Enqueued {(iy, ix)} / {queue.Count}");
                }
            }

            var allPoints = visited.ToArray();
            int sy = 0, sx = 0;
            for (int i = 0; i < allPoints.Length; i++)
            {
                sy += allPoints[i].Y;
                sx += allPoints[i].X;
            }

            return new ComponentData(id, area, (sy / allPoints.Length, sx / allPoints.Length), (0, 0), allPoints);
        }

        public static unsafe ConnectedComponents FindInMatrix(int[] mat, int rows, int cols, ProgressMonitor progressMonitor = null, string taskName = null)
        {            
            progressMonitor?.PushTask(taskName ?? "ConComp", rows);

            var map = new int[rows * cols];

            var components = new List<ComponentData>();

            fixed (int* pmap = &map[0])
            {
                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < cols; x++)
                    {
                        var pos = y * cols + x;
                        if (pmap[pos] != 0 || mat[pos] == 0) 
                            continue;
                        components.Add(FindOneComponentInMatrix(mat, rows, cols, pmap, y, x, components.Count + 1));
                    }
                    progressMonitor?.AdvanceOneStep();
                }
            }
            progressMonitor?.PopTask();
            return new ConnectedComponents(map, components.ToArray(), rows, cols);
        }

    }
}
