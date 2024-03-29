using LillyScan.Backend.Math;

namespace LillyScan.Backend.AI.Activations
{
    public abstract class Activation
    {
        public abstract Tensor<float> Call(Tensor<float> input);
    }
}
