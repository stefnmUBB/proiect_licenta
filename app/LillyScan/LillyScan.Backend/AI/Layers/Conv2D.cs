using LillyScan.Backend.Math;
using System.Linq;

namespace LillyScan.Backend.AI.Layers
{
    internal class Conv2D : Layer
    {        
        readonly int Filters;
        readonly (int Rows, int Cols) KernelSize;        
        readonly bool UseBias;

        public Conv2D(string name, int filters, (int, int) kernelSize, bool useBias) : base(name)
        {
            (Filters, KernelSize, UseBias) = (filters, kernelSize, useBias);            
        }

        public override Tensor<float>[] Call(Tensor<float>[] inputs)
        {
            Assert(() => inputs != null, () => inputs.Length == 1);
            var input = inputs[0];
            Assert(() => input.Rank == 4);
            var kernel = Context.GetWeight("kernel", (KernelSize.Rows, KernelSize.Cols, input.Shape[-1], Filters));

            var output = input.Conv2D(kernel);
            if(UseBias)
            {
                var bias = Context.GetWeight("bias", (Filters));
                output = output.Add(bias);
            }
            return new[] { output };
        }

        public override Shape GetOutputShape(Shape[] inputShapes)
        {
            Assert(() => inputShapes != null, () => inputShapes.Length == 1);            
            var shape = inputShapes[0].ToArray();
            Assert(() => shape.Length == 4);
            shape[3] = Filters;
            return new Shape(shape);
        }
    }
}
