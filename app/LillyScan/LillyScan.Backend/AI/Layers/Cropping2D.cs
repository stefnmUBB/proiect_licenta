using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
using LillyScan.Backend.Types;

namespace LillyScan.Backend.AI.Layers
{
    [Named("Cropping2D")]
    public class Cropping2D : FeatureMap2DLayer
    {
        [TfConfigProperty("cropping", converter: typeof(Cropping2DConverter))]
        public (int Top, int Bottom, int Left, int Right) Cropping { get; private set; }

        internal Cropping2D() { }
        public Cropping2D(Shape[] inputShapes, (int Top, int Bottom, int Left, int Right) cropping, string name=null)
            : base(inputShapes, name) 
        {
            Cropping = cropping;
        }


        public override Shape[] OnGetOutputShape(Shape[] inputShapes)
        {
            var shape = inputShapes[0];
            return new Shape[] { (shape[0], shape[1] - Cropping.Top - Cropping.Bottom, shape[2] - Cropping.Left - Cropping.Right, shape[3]) };
        }

        protected override unsafe Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            var inputBuffer = inputs[0].Buffer.Buffer;
            var inputShape = inputs[0].Shape;
            (var B, var H, var W, var C) = (inputShape[0], inputShape[1], inputShape[2], inputShape[3]);
            var resultShape = new Shape(B, H - Cropping.Top - Cropping.Bottom, W - Cropping.Left - Cropping.Right, C);
            var result = new float[resultShape.ElementsCount];

            int h0 = Cropping.Top, h1 = H - Cropping.Bottom;
            int w0 = Cropping.Left, w1 = W - Cropping.Right;

            fixed (float* presult = &result[0])
            {
                float* iresult = presult;
                for(int b=0;b<B;b++)
                {
                    int k0 = b * H * W * C;
                    for(int i=h0;i<h1;i++)
                    {
                        int k1 = k0 + i * W * C;
                        for(int j=w0;j<w1;j++)
                        {
                            int k2 = k1 + j * C;
                            for(int c=0;c<C;c++)
                            {
                                *iresult++ = inputBuffer[k2 + c];
                            }
                        }
                    }
                }
            }
            return new[] { new Tensor<float>(resultShape, result) };
        }

        protected override void OnValidateInputShapes(Shape[] inputShapes)
        {
            base.OnValidateInputShapes(inputShapes);
            Assert("Invalid Cropping2D shape", inputShapes[0][1] > Cropping.Top + Cropping.Bottom && inputShapes[0][2] > Cropping.Left + Cropping.Right);
        }
    }
}
