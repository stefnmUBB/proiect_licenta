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

        internal Activation() { }
        public Activation(Shape[] inputShapes, Activations.Activation activation, string name=null) : base(inputShapes, name)
        {
            ActivationFunction = activation;
        }

        public Activation(Shape[] inputShapes, string activationName, string name = null) 
            :this(inputShapes, ActivationFunctionConverter.Convert(activationName), name)
        { }

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            return inputs.Select(ActivationFunction.Call).ToArray();
        }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes) => inputShapes;        
    }
}
