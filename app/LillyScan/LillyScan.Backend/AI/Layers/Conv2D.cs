using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using System.Linq;

namespace LillyScan.Backend.AI.Layers
{
    [Named("Conv2D")]
    public class Conv2D : Layer
    {
        [TfConfigProperty("filters")]
        public int Filters { get; private set; }

        [TfConfigProperty("kernel_size", converter:typeof(IntValueTuple2Converter))]
        public (int Rows, int Cols) KernelSize { get; private set; }

        [TfConfigProperty("use_bias")]
        public bool UseBias { get; private set; }

        internal Conv2D() { }

        public Conv2D(Shape[] inputShapes, int filters, (int, int)? kernelSize = null, bool useBias = true, string name = null) : base(inputShapes, name)
        {
            Assert(() => inputShapes.Length == 1, () => inputShapes[0].Length == 4);
            (Filters, KernelSize, UseBias) = (filters, kernelSize.HasValue ? kernelSize.Value : (1, 1), useBias);
        }

        public Conv2D(Shape inputShape, int filters, (int, int)? kernelSize = null, bool useBias = true, string name = null)
            : this(new[] {inputShape}, filters, kernelSize, useBias, name)
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
    }
}
