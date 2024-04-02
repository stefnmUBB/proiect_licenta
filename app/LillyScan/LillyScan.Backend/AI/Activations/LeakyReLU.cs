using LillyScan.Backend.Math;
using LillyScan.Backend.Types;

namespace LillyScan.Backend.AI.Activations
{
    [Named("leaky_relu")]
    internal class LeakyReLU : Activation
    {
        private readonly float NegativeSlope;

        public LeakyReLU(float negativeSlope)
        {
            NegativeSlope = negativeSlope;
        }

        public LeakyReLU() : this(0.2f) { }

        public override Tensor<float> Call(Tensor<float> input)
        {                        
            return input.Map(x => x < 0 ? NegativeSlope * x : x);
        }
    }
}
