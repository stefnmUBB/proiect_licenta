using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
using LillyScan.Backend.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.AI.Layers
{
    public class Pooling2D : FeatureMap2DLayer
    {
        [TfConfigProperty("pool_size",converter:typeof(IntValueTuple2Converter))]
        public (int Rows,int Cols) PoolSize { get; private set; }

        [TfConfigProperty("strides", converter: typeof(IntValueTuple2Converter))]
        public (int Rows, int Cols) Strides { get; private set; }

        [TfConfigProperty("padding", converter: typeof(PaddingConverter))]
        public Padding Padding { get; private set; } = Padding.Valid;

        protected Func<float[], float> PoolingOperation { get; }


        internal Pooling2D(Func<float[], float> poolingOperation) 
        {
            PoolingOperation = poolingOperation;
        }

        public Pooling2D(Func<float[], float> poolingOperation, Shape[] input_shapes, (int, int) poolSize, (int, int)? strides = null, Padding padding = Padding.Valid, string name = null)
            : base(input_shapes, name)
        {
            PoolingOperation = poolingOperation;
            PoolSize = poolSize;
            Strides = strides ?? poolSize;
            Padding = padding;
        }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes)
        {
            var shape = inputShapes[0];
            switch(Padding)
            {
                case Padding.Valid:
                    {
                        int h = (shape[1] - PoolSize.Rows) / Strides.Rows + 1;
                        int w = (shape[2] - PoolSize.Cols) / Strides.Cols + 1;
                        return new[] { new Shape(shape[0], h, w, shape[3]) };
                    }                    
                case Padding.Same:
                    {
                        int h = (shape[1] - 1) / Strides.Rows + 1;
                        int w = (shape[2] - 1) / Strides.Cols + 1;
                        return new[] { new Shape(shape[0], h, w, shape[3]) };
                    }
                default:
                    throw new NotImplementedException();
            }            
        }

        private static IEnumerable<int> EnumerateWindowed(int count, int pool_size, int strides)
        {
            for (int i = pool_size / 2; i <= count - (pool_size - pool_size / 2); i += strides)
                yield return i;
        }

        private static IEnumerable<(int, int)> EnumeratePool(int i, int j, (int Rows, int Cols) pool_size, (int Rows, int Cols) offset = default)
        {
            for (int ii = i - pool_size.Rows / 2 + offset.Rows; ii < i + (pool_size.Rows - pool_size.Rows / 2) + offset.Rows; ii++)
                for (int jj = j - pool_size.Cols / 2 + offset.Cols; jj < j + (pool_size.Cols - pool_size.Cols / 2) + offset.Cols; jj++) 
                    yield return (ii, jj);
        }

        private Tensor<float> SolveSingleMap(Tensor<float> input) 
        {
            Assert(() => input.Rank == 3); // shape (h,w,c)
            var shape = input.Shape;

            switch (Padding)
            {
                case Padding.Valid:
                    {
                        int h = (shape[0] - PoolSize.Rows) / Strides.Rows + 1;
                        int w = (shape[1] - PoolSize.Cols) / Strides.Cols + 1;
                        var outShape = new Shape(h, w, shape[2]);
                        var buffer = new float[outShape.ElementsCount];

                        int k = 0;
                        foreach(var i in EnumerateWindowed(shape[0], PoolSize.Rows, Strides.Rows))
                        {
                            foreach (var j in EnumerateWindowed(shape[1], PoolSize.Cols, Strides.Cols))
                            {
                                for (int c = 0; c < shape[2]; c++)
                                {
                                    var values = EnumeratePool(i, j, PoolSize)
                                        .Select(pq => input.GetValueAt(pq.Item1, pq.Item2, c))
                                        .ToArray();
                                    buffer[k++] = PoolingOperation(values);
                                }                                
                            }
                        }
                        return new Tensor<float>(outShape, buffer);
                    }
                case Padding.Same:
                    {
                        int h = (shape[0] - 1) / Strides.Rows + 1;
                        int w = (shape[1] - 1) / Strides.Cols + 1;
                        var outShape = new Shape(h, w, shape[2]);
                        var buffer = new float[outShape.ElementsCount];

                        int k = 0;
                        for (int i= 0; i < shape[0];i+=Strides.Rows)
                        {
                            for (int j = 0; j < shape[1]; j += Strides.Cols) 
                            {
                                for (int c = 0; c < shape[2]; c++)
                                {
                                    var values = EnumeratePool(i, j, PoolSize, offset: (1 - PoolSize.Rows % 2, 1 - PoolSize.Cols % 2))
                                        .Where(pq => pq.Item1.IsBetween(0, shape[0] - 1) && pq.Item2.IsBetween(0, shape[1] - 1))
                                        .Select(pq => input.GetValueAt(pq.Item1, pq.Item2, c))
                                        .ToArray();
                                    buffer[k++] = PoolingOperation(values);
                                }
                            }
                        }

                        return new Tensor<float>(outShape, buffer);

                    }
                default:
                    throw new NotImplementedException();
            }

        }

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            return new[] { Tensors.Stack(inputs[0].Unstack(axis: 0).Select(SolveSingleMap)) };
        }

        protected override void OnValidateInputShapes(Shape[] inputShapes)
        {
            base.OnValidateInputShapes(inputShapes);
            var shape = inputShapes[0];
            if (Padding == Padding.Valid)
                Assert(() => shape[1] >= PoolSize.Rows, () => shape[2] >= PoolSize.Cols);
        }
    }
}
