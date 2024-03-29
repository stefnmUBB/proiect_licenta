using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.Math
{
    public class Tensor<T>
    {
        public Shape Shape { get; }
        public ImmutableArray<T> Buffer { get; }

        public Tensor(Shape shape, ImmutableArray<T> buffer)
        {
            if (buffer.Length != shape.ElementsCount)
                throw new ArgumentException($"Elements count ({buffer.Length}) does not match shape {Shape}");
            (Shape, Buffer) = (shape, buffer);
        }        

        public Tensor(Shape shape, T[] buffer) : this(shape, new ImmutableArray<T>(buffer)) { }

        public Tensor(Shape shape) : this(shape, new ImmutableArray<T>(new T[shape.ElementsCount])) { }        

        public Tensor<T> this[params ISequenceAccessor[] acc] => GetSlices(acc);

        public T GetValueAt(params int[] indices) => Buffer[Shape.GetBufferIndex(indices)];        
        public T GetValueAt(ImmutableArray<int> indices) => Buffer[Shape.GetBufferIndex(indices)];

        public Tensor<T> GetFromBatches(int[] indices)
        {
            if (indices.Length >= Rank)
                throw new ArgumentException("Invalid batch: too many indices");

            var shape = new Shape(Shape.Skip(indices.Length).ToArray());
            var buffStartIndex = Shape.GetBufferIndex(indices.Concat(new int[Rank - indices.Length]).ToArray());
            var buffer = new T[shape.ElementsCount];
            Buffer.CopyTo(buffStartIndex, buffer, 0, buffer.Length);
            return new Tensor<T>(shape, buffer);
        }

        public Tensor<T> GetFromBatches(ImmutableArray<int> indices)
        {
            if (indices.Length >= Rank)
                throw new ArgumentException("Invalid batch: too many indices");

            var shape = new Shape(Shape.Skip(indices.Length).ToArray());
            Console.WriteLine($"Shape={shape}/{Shape}");
            Console.WriteLine(Buffer);
            var buffStartIndex = Shape.GetBufferIndex(indices.Concat(new int[Rank - indices.Length]).ToArray());
            Console.WriteLine($"indices={indices}");
            Console.WriteLine($"si={buffStartIndex}");
            var buffer = new T[shape.ElementsCount];
            Buffer.CopyTo(buffStartIndex, buffer, 0, buffer.Length);
            return new Tensor<T>(shape, buffer);
        }



        public Tensor<T> GetSlices(params ISequenceAccessor[] acc)
        {
            if (acc.Length > Shape.DimsCount)
                throw new ArgumentException("More slices than dimensions");

            acc = acc.Select(_ => _ ?? new Slice(null, null, null)).Concat(Enumerable.Repeat(new Slice(null, null, null), Shape.DimsCount - acc.Length)).ToArray();

            var newShape = new List<int>();

            var ixList = new List<int[]>();
            for(int i=0;i<acc.Length;i++)
            {
                var ixs = acc[i].GetIndices(Shape[i]);
                ixList.Add(ixs);
                if (!acc[i].DimReduce)
                    newShape.Add(ixs.Length);
            }

            var iter = new int[Shape.DimsCount];            

            var buffer = new List<T>();

            while (iter[0] < ixList[0].Length) 
            {
                var elem = Buffer[Shape.GetBufferIndex(iter.Select((x, i) => ixList[i][x]).ToArray())];
                buffer.Add(elem);

                for (int i=Shape.DimsCount-1,c=1;i>=0 && c>0;i--)
                {
                    iter[i]++;
                    c = 0;
                    if (i > 0 && iter[i] == ixList[i].Length) 
                    {
                        iter[i] = 0;
                        c = 1;
                    }
                }                
            }

            return new Tensor<T>(newShape.ToArray(), new ImmutableArray<T>(buffer));
        }

        public Tensor<T> Print(string message = "", TextWriter w = null)
        {
            w = w ?? Console.Out;
            var sb = new StringBuilder();
            var iter = new int[Shape.DimsCount];            

            sb.Append(message);
            sb.AppendLine($"Tensor of type {typeof(T)} and shape {Shape}:");
            sb.Append(new string('[', Shape.DimsCount));
            while (iter[0] < Shape[0])
            {
                var elem = Buffer[Shape.GetBufferIndex(iter)];
                sb.Append(elem);                
                //Console.WriteLine(iter.JoinToString(" "));
                int b = 0;
                for (int i = Shape.DimsCount - 1, c = 1; i >= 0 && c > 0; i--) 
                {
                    iter[i]++;
                    c = 0;
                    if (i > 0 && iter[i] == Shape[i])
                    {
                        iter[i] = 0;
                        c = 1;
                        b++;
                        sb.Append("]");
                    }                    
                }
                if(b==0)
                    sb.Append(" ");
                if (b > 0 && iter[0] < Shape[0]) 
                    sb.Append("\n" + new string('[', b));
            }
            sb.Append(']');
            w.WriteLine(sb);

            return this;
        }        

        public int Rank => Shape.DimsCount;                
    }    
}
