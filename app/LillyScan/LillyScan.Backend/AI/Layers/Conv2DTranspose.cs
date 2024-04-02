using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using System;
using System.Linq;

namespace LillyScan.Backend.AI.Layers
{
    [Named("Conv2DTranspose")]
    public class Conv2DTranspose : FeatureMap2DLayer
    {
        [TfConfigProperty("filters")]
        public int Filters { get; private set; }

        [TfConfigProperty("kernel_size", converter: typeof(IntValueTuple2Converter))]
        public (int Rows, int Cols) KernelSize { get; private set; }

        [TfConfigProperty("strides", converter: typeof(IntValueTuple2Converter))]
        public (int Rows, int Cols) Strides { get; private set; }

        [TfConfigProperty("use_bias")]
        public bool UseBias { get; private set; }

        [TfConfigProperty("padding", converter: typeof(PaddingConverter))]
        public Padding Padding { get; private set; }

        internal Conv2DTranspose() { }

        private Shape KernelShape => (KernelSize.Rows, KernelSize.Cols, Filters, InputShapes[0][-1]);
        private Shape BiasShape => (Filters);

        public Conv2DTranspose(Shape[] inputShapes, int filters, (int, int)? kernelSize = null, (int, int)? strides = null, bool useBias = true, string name = null)
            : base(inputShapes, name)
        {
            (Filters, KernelSize, Strides, UseBias) = (filters, kernelSize ?? (1, 1), strides ?? (1, 1), useBias);

            Context.Weights["kernel"] = Tensors.Ones<float>(KernelShape);
            if (UseBias)
            {
                Context.Weights["bias"] = Tensors.Zeros<float>(BiasShape);
            }
        }

        public Conv2DTranspose(Shape inputShape, int filters, (int, int)? kernelSize = null, (int, int)? strides = null, bool useBias = true, string name = null)
            : this(new[] { inputShape }, filters, kernelSize, strides, useBias, name) { }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes)
        {
            var shape = inputShapes[0].ToArray();
            shape[1] *= Strides.Rows;
            shape[2] *= Strides.Cols;
            shape[3] = Filters;
            return new[] { new Shape(shape) };
        }

        
        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            if (Padding == Padding.Valid)
                throw new NotImplementedException("Padding.Valid is not implemented. Use Padding.Same");
            var input = inputs[0];            
            var kernel = Context.GetWeight("kernel", KernelShape, false);            

            var output = input.Conv2DTransposeFloat32(kernel, Strides);
            if (UseBias)
            {
                var bias = Context.GetWeight("bias", BiasShape, false);
                output = output.Add(bias);
            }
            return new[] { output };            
        }
        public override void LoadWeights(Tensor<float>[] weights)
        {
            Context.Weights["kernel"] = weights[0];
            if (UseBias)
                Context.Weights["bias"] = weights[1];
        }
    }
}
