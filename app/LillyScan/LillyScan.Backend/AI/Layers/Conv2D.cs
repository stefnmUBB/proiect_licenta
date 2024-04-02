using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using System.Linq;

namespace LillyScan.Backend.AI.Layers
{
    [Named("Conv2D")]
    public class Conv2D : FeatureMap2DLayer
    {
        [TfConfigProperty("filters")]
        public int Filters { get; private set; }

        [TfConfigProperty("kernel_size", converter:typeof(IntValueTuple2Converter))]
        public (int Rows, int Cols) KernelSize { get; private set; }

        [TfConfigProperty("use_bias")]
        public bool UseBias { get; private set; }

        [TfConfigProperty("activation",converter:typeof(ActivationFunctionConverter))]
        public Activations.Activation Activation { get; private set; }

        internal Conv2D() { }

        public Conv2D(Shape[] inputShapes, int filters, (int, int)? kernelSize = null, bool useBias = true, string name = null, Activations.Activation activation = null) : base(inputShapes, name)
        {
            Assert(() => inputShapes.Length == 1, () => inputShapes[0].Length == 4);
            (Filters, KernelSize, UseBias) = (filters, kernelSize ?? (1, 1), useBias);

            Context.Weights["kernel"] = Tensors.Ones<float>((KernelSize.Rows, KernelSize.Cols, InputShapes[0][-1], Filters));
            if (UseBias)
            {
                Context.Weights["bias"] = Tensors.Zeros<float>((Filters));
            }
            Activation = activation;
        }

        public Conv2D(Shape inputShape, int filters, (int, int)? kernelSize = null, bool useBias = true, string name = null, Activations.Activation activation = null)
            : this(new[] { inputShape }, filters, kernelSize, useBias, name, activation)
        { }

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            var input = inputs[0];
            var kernel = Context.GetWeight("kernel", (KernelSize.Rows, KernelSize.Cols, input.Shape[-1], Filters), false);

            var output = input.Conv2D(kernel);
            if(UseBias)
            {
                var bias = Context.GetWeight("bias", (Filters), false);
                output = output.Add(bias);
            }

            if(Activation != null)
            {
                output = Activation.Call(output);
            }

            return new[] { output };
        }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes)
        {
            var shape = inputShapes[0].ToArray();            
            shape[3] = Filters;
            return new[] { new Shape(shape) };
        }

        public override void LoadFromConfig(TfConfig config)
        {
            base.LoadFromConfig(config);
        }

        public override void LoadWeights(Tensor<float>[] weights)
        {
            Context.Weights["kernel"] = weights[0];
            if (UseBias)
                Context.Weights["bias"] = weights[1];
        }
    }
}
