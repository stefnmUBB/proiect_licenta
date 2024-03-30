using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
using System;

namespace LillyScan.Backend.AI.Layers
{
    public class Pooling2D : FeatureMap2DLayer
    {
        [TfConfigProperty("pool_size",converter:typeof(IntValueTuple2Converter))]
        public (int Rows,int Cols) PoolSize { get; private set; }

        [TfConfigProperty("strides", converter: typeof(IntValueTuple2Converter))]
        public (int Rows, int Cols) Strides { get; private set; }

        [TfConfigProperty("padding", converter: typeof(PaddingConverter))]
        public Padding Padding { get; private set; }

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

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            var input = inputs[0];
            var shape = input.Shape;

            switch (Padding)
            {
                case Padding.Valid:
                    {
                        int h = (shape[1] - PoolSize.Rows) / Strides.Rows + 1;
                        int w = (shape[2] - PoolSize.Cols) / Strides.Cols + 1;
                        var outShape = new Shape(shape[0], h, w, shape[3]);
                        var buffer = new Tensor<float>(outShape);

                        //input.SubDimBroadcast(null,)
                        return null;                        
                    }
                case Padding.Same:
                    {
                        int h = (shape[1] - 1) / Strides.Rows + 1;
                        int w = (shape[2] - 1) / Strides.Cols + 1;
                        var outShape = new Shape(shape[0], h, w, shape[3]);
                        return null;
                        
                    }
                default:
                    throw new NotImplementedException();
            }
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
