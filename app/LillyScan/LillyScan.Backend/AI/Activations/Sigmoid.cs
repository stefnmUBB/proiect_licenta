using LillyScan.Backend.Math;
using LillyScan.Backend.Types;

namespace LillyScan.Backend.AI.Activations
{
    [Named("sigmoid")]
    public class Sigmoid : Activation
    {
        public override Tensor<float> Call(Tensor<float> input)
        {
            return input.Map(x => (float)(1 / (1 + System.Math.Exp(-x))));
        }
    }
}
