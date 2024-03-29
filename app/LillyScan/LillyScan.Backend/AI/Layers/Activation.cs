using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using System.Linq;

namespace LillyScan.Backend.AI.Layers
{
    [Named("Activation")]
    public class Activation : Layer
    {
        [TfConfigProperty("activation", converter:typeof(ActivationFunctionConverter))]
        public Activations.Activation ActivationFunction { get; private set; }

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            return inputs.Select(ActivationFunction.Call).ToArray();
        }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes)
        {
            return inputShapes;
        }        
    }
}
