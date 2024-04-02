using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
using LillyScan.Backend.Types;

namespace LillyScan.Backend.AI.Layers
{
    [Named("UpSampling2D")]
    public class UpSampling2D : FeatureMap2DLayer
    {
        [TfConfigProperty("size",converter:typeof(IntValueTuple2Converter))]
        public (int Rows, int Cols) Size { get; private set; }
        
        internal UpSampling2D() { }
        public UpSampling2D(Shape[] inputShapes, (int Rows, int Cols) size, string name=null) : base(inputShapes, name)
        {
            Size = size;
        }

        public UpSampling2D(Shape inputShape, (int Rows, int Cols) size, string name = null)
            : this(new[] { inputShape }, size, name) { }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes)
        {
            var shape = inputShapes[0];
            return new[] { new Shape(shape[0], Size.Rows * shape[1], Size.Cols * shape[2], shape[3]) };
        }

        protected override unsafe Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            var input = inputs[0];
            (int batchSize, int height, int width, int channels) = (input.Shape[0], input.Shape[1], input.Shape[2], input.Shape[3]);

            var outputShape = new Shape(batchSize, Size.Rows * height, Size.Cols * width, channels);
            var buffer = new float[outputShape.ElementsCount];

            fixed(float* tbuf = &input.Buffer.Buffer[0])
            fixed(float* rbuf = &buffer[0])
            {
                UnsafeOperations.UpSampling2D(tbuf, rbuf, batchSize, height, width, channels, Size.Rows, Size.Cols);
            }

            /*foreach(var ix in input.Shape.IterateIndices())
            {
                (int b, int i, int j, int c) = (ix[0], ix[1], ix[2], ix[3]);

                for(int p=0;p<Size.Rows;p++)
                {
                    for(int q=0;q<Size.Cols;q++)
                    {
                        var index = outputShape.GetBufferIndex(b, i * Size.Rows + p, j * Size.Cols + q, c);
                        buffer[index] = input.GetValueAt(ix);
                    }
                }
            }*/
            return new[] { new Tensor<float>(outputShape, buffer) };
        }
    }
}
