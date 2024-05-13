using LillyScan.Backend.Imaging;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.HTR
{
    internal class MaskKMeans
    {
        public static int[] FindClusters(LocalizedMask[] masks, int k, float v1, float v2, Action<int[], (float X, float Y)[]> callback = null)
        {
            if (masks.Length == 0)
                return new int[0];
            var gamma = 362;
            float area(float a, float b, float c, float x, float y, float l)
            {
                var h = System.Math.Abs(a * x + b * y + c);
                return h * l / 2;
            }

            (float X, float Y) changeBasis(float x, float y) => (y * v1 - x * v2, x * v1 + y * v2); 
            

            float distance(float x1, float y1, float x2, float y2, LocalizedMask mask) 
            {
                (var a1, var b1) = changeBasis(x1, y1);
                (var a2, var b2) = changeBasis(x2, y2);                
                var da = a2 - a1;
                var db = System.Math.Abs(b2 - b1);
                var a = area(mask.LineFit.A, mask.LineFit.B, mask.LineFit.C, x1, y1, mask.FitSegmentLength);
                return (float)System.Math.Sqrt(da * da + gamma * db * db * db * db);
            }            

            var centroids = new (float X, float Y)[k];

            //var centers = masks.Select(_ => (X:_.CenterX, Y:_.CenterY)).ToArray();
            var centers = masks.Select(_ => changeBasis(_.CenterX, _.CenterY)).ToArray();

            (float minCX, float maxCX) = centers.MinAndMax(_ => _.X);
            (float minCY, float maxCY) = centers.MinAndMax(_ => _.Y);            

            if (masks.Length <= k) 
            {
                for (int i = 0; i < masks.Length; i++)
                    centroids[i] = (masks[i].CenterX, masks[i].CenterY);
                for (int i = masks.Length; i < k; i++)
                    centroids[i] = changeBasis(minCX + (maxCX - minCX) * i / (k - 1), maxCY + (minCY - maxCY) * i / (k - 1));
            }
            else
            {
                for (int i = 0; i < k; i++)
                    centroids[i] = changeBasis(minCX + (maxCX - minCX) * i / (k - 1), maxCY + (minCY - maxCY) * i / (k - 1));                    
            }

            int closestCentroid(LocalizedMask mask, out float rCost)
            {
                int ri = 0;
                rCost = distance(centroids[0].X, centroids[0].Y, mask.CenterX, mask.CenterY, mask);
                for(int i=1;i<centroids.Length;i++)
                {
                    float cost = distance(centroids[i].X, centroids[i].Y, mask.CenterX, mask.CenterY, mask);
                    if(cost<rCost)
                    {
                        (ri, rCost) = (i, cost);
                    }
                }
                return ri;
            }

            float prevCost = -100, newCost = 0;
            var result = new int[masks.Length];
            var clusters = new List<LocalizedMask>[k];
            for (int i = 0; i < k; i++) clusters[i] = new List<LocalizedMask>();

            var rand = new Random();
            //Console.WriteLine("Cluster invoke!!!!!!!!!!!!!!!!!!!!!!!!!11");
            //callback?.Invoke(result, centroids);

            int epoch = 0;
            while (System.Math.Abs(prevCost - newCost) > 1 && epoch < 50) 
            {
                prevCost = newCost;
                for (int i = 0; i < k; i++)
                    clusters[i].Clear();

                newCost = 0;
                for(int i=0;i<masks.Length;i++)
                {
                    result[i] = closestCentroid(masks[i], out var cost);
                    clusters[result[i]].Add(masks[i]);
                    newCost += cost;
                }

                for(int i=0;i<k;i++)
                {
                    if (clusters[i].Count == 0)
                    {
                        //centroids[i] = (256 * (float)rand.NextDouble(), 256 * (float)rand.NextDouble());
                        centroids[i] = ((float)rand.NextDouble(), (float)rand.NextDouble());
                        continue;
                    }
                    //var totalArea = clusters[i].Sum(_ => _.Area);
                    //var x = clusters[i].Sum(_ => _.CenterX * _.Area) / totalArea; // clusters[i].Count;
                    //var y = clusters[i].Sum(_ => _.CenterY * _.Area) / totalArea; // clusters[i].Count;
                    var x = clusters[i].Sum(_ => _.CenterX) / clusters[i].Count;
                    var y = clusters[i].Sum(_ => _.CenterY) / clusters[i].Count;
                    centroids[i] = (x, y);
                }
                epoch++;
                
            }

            for (int i = 0; i < masks.Length; i++)            
                result[i] = closestCentroid(masks[i], out var cost);                            

            Console.WriteLine("Cluster invoke!!!!!!!!!!!!!!!!!!!!!!!!!11");
            callback?.Invoke(result, centroids);


            return result;
        }

        public static int coeff2 = 0;
        public static int coeff1 = 0;

        public static float PartitionScore(LocalizedMask[] masks, int[] clusters, float v1, float v2, Action<float, float, float, float, float, float, float, float> rect=null)
        {                   
            Dictionary<int, List<LocalizedMask>> partitions = new Dictionary<int, List<LocalizedMask>>();

            int clustersCount = clusters.Distinct().Count();

            for(int i=0;i<masks.Length;i++)            
                partitions.GetOrCreate(clusters[i]).Add(masks[i]);

            float score = 0, centerDiffScore=0;

            var cvt = new Func<float, float, (float X, float Y)>((float x, float y) => (y * v1 - x * v2, x * v1 + y * v2));

            foreach (var k in partitions.Keys) 
            {
                var m = partitions[k];               
                float centerDiff = 0;                

                foreach (var _ in m)
                {
                    var cx = _.CenterY * v1 - _.CenterX * v2;
                    var cy = _.CenterX * v1 + _.CenterY * v2;
                    var w = _.FitSegmentLength / 2;
                    var h = _.MaxLineDistanceError;                    
                }

                (var cxmin, var cxmax) = m.MinAndMax(_ => _.CenterY * v1 - _.CenterX * v2);
                (var cymin, var cymax) = m.MinAndMax(_ => _.CenterX * v1 + _.CenterY * v2);

                var corners = m.Select(_ => (x: _.CenterY * v1 - _.CenterX * v2, y: _.CenterX * v1 + _.CenterY * v2, w: _.FitSegmentLength / 2, h: (float)System.Math.Sqrt(_.MaxLineDistanceError)))
                    .SelectMany(_ => new[] { (x: _.x - _.w, y: _.y - _.h), (x: _.x + _.w, y: _.y + _.h) }).ToArray();

                var ctrX = m.Sum(_ => _.CenterX) / m.Count;
                var ctrY = m.Sum(_ => _.CenterY) / m.Count;
                (ctrX, ctrY) = cvt(ctrX, ctrY);

                (var x0, var x1) = corners.MinAndMax(_ => _.x); //(_ => _.y * v1 - _.x * v2);
                (var y0, var y1) = corners.MinAndMax(_ => _.y); //(_ => _.x * v1 + _.y * v2);
                var dy = y1 - y0;
                var dx = x1 - x0;

                centerDiff += dy == 0 ? 0 : (cymax - cymin);// / dy;

                //Console.WriteLine($"   Y01 = {y0} {y1}");                

                (var bx0, var by0) = cvt(x0, y0);
                (var bx1, var by1) = cvt(x0, y1);
                (var bx2, var by2) = cvt(x1, y1);
                (var bx3, var by3) = cvt(x1, y0);
                rect?.Invoke(bx0, by0, bx1, by1, bx2, by2, bx3, by3);

                //rect?.Invoke(y0 * v1 - x0 * v2, x0 * v1 + y0 * v2, y1 * v1 - x1 * v2, x1 * v1 + y1 * v2);

                score += dy/256; // dy*dy / m.Count;
                centerDiffScore = System.Math.Max(centerDiffScore, centerDiff);  //+= centerDiff;
                var rectArea = (x1 - x0) * (float)System.Math.Pow((y1 - y0), 1 + 1.0 / m.Count);
                //score += rectArea;
            }

            //return score + 3 * centerDiffScore + clustersCount * 0.5f;

            var classes = clusters.Distinct().ToArray();

            Dictionary<int, float> innerScore=new Dictionary<int, float>();            
            Dictionary<int, float> outerScore = new Dictionary<int, float>();

            float a = 2;
            float b = 13;

            Func<float, float> f1 = t => (float)System.Math.Pow(t, a);
            Func<float, float> f2 = t => (float)System.Math.Pow(1 + t, -b);

            float diag = (float)(256 * System.Math.Sqrt(2));

            /*for(int i=0;i<masks.Length;i++)
            {
                for(int j=i+1;j<masks.Length;j++)
                {

                }
            }*/

            for(int q=0;q<classes.Length;q++)
            {
                int c1 = classes[q];
                innerScore[c1] = 0;
                outerScore[c1] = 0;

                var inClass = Enumerable.Range(0, masks.Length).Where(i => clusters[i] == c1).Select(i => masks[i]).ToArray();
                var outClass = Enumerable.Range(0, masks.Length).Where(i => clusters[i] != c1).Select(i => masks[i]).ToArray();

                for(int i=0;i<inClass.Length;i++)
                {
                    var yi = cvt(inClass[i].CenterX, inClass[i].CenterY).Y;
                    for (int j = 0; j < inClass.Length; j++) 
                    {
                        if (j == i) continue;
                        var yj = cvt(inClass[j].CenterX, inClass[j].CenterY).Y;
                        var dy = System.Math.Abs(yj - yi) / diag;
                        innerScore[c1] += f1(dy);
                    }
                }

                for (int i = 0; i < inClass.Length; i++)
                {
                    var yi = cvt(inClass[i].CenterX, inClass[i].CenterY).Y;
                    for (int j = 0; j < outClass.Length; j++) 
                    {
                        var yj = cvt(outClass[j].CenterX, outClass[j].CenterY).Y;
                        var dy = System.Math.Abs(yj - yi) / diag;
                        outerScore[c1] += f2(dy);
                    }
                }

                /*for (int i=0;i<masks.Length;i++)
                {
                    var yi = cvt(masks[i].CenterX, masks[i].CenterY).Y;
                    if (clusters[i] == c1)
                    {
                        for (int j = i+1; j < masks.Length; j++) 
                        {                            
                            if (clusters[j] != c1) continue;
                            var yj = cvt(masks[j].CenterX, masks[j].CenterY).Y;
                            var dy = System.Math.Abs(yj - yi) / diag;
                            //innerScore[c1] = System.Math.Max(innerScore[c1], f1(dy));
                            innerScore[c1] += f1(dy);
                        }
                    }
                    else
                    {
                        var dist = float.PositiveInfinity;
                        for(int j=0;j<masks.Length;j++)
                        {
                            if (clusters[j] != c1) continue;
                            var yj = cvt(masks[j].CenterX, masks[j].CenterY).Y;
                            var dy = System.Math.Abs(yj - yi) / diag;
                            dist = System.Math.Min(dist, dy);
                            outerScore[c1] += f2(dy);
                        }
                        //Console.WriteLine($"min found = {dist} | {f2(dist)}");                        
                    }
                }*/
            }

            float fragScore=0;
            foreach(var c in classes)
            {
                Console.WriteLine($"IO SCORE : {(innerScore[c], outerScore[c])}");
                //fragScore += innerScore[c] * innerScore[c] + outerScore[c] * outerScore[c];
                fragScore += innerScore[c] + outerScore[c];
            }

            return fragScore;
        }        
    }
}
