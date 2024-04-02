using LillyScan.Backend.Math;
using LillyScan.Backend.Types;

namespace LillyScan.Backend.AI.Activations
{
    [Named("linear")]
    public class Linear : Activation
    {
        public override Tensor<float> Call(Tensor<float> input) => input;        
    }
}
