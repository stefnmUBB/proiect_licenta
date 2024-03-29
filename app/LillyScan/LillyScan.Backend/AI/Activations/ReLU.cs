using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using LillyScan.Backend.Utils;

namespace LillyScan.Backend.AI.Activations
{
    [Named("relu")]
    public class ReLU : Activation
    {
        private readonly float? MaxValue;
        private readonly float NegativeSlope;
        private readonly float Threshold;

        public ReLU(float? maxValue = null, float negativeSlope = 0, float threshold = 0)
        {
            MaxValue = maxValue;
            NegativeSlope = negativeSlope;
            Threshold = threshold;
        }

        public override Tensor<float> Call(Tensor<float> input)
        {
            return input.Map(x =>
            {
                float negativePart = 0;
                if(NegativeSlope!=0)
                {
                    if (!MaxValue.HasValue && Threshold == 0)
                        return x < 0 ? NegativeSlope * x : x; // leaky_relu
                    negativePart = Threshold != 0 ? System.Math.Max(-x + Threshold, 0) : System.Math.Max(-x, 0);
                }

                bool clipMax = MaxValue.HasValue;

                if(Threshold!=0)
                {
                    x *= (x > Threshold) ? 1 : 0;
                }
                else if(MaxValue==6)
                {
                    x = x.Clamp(0, 6);
                    clipMax = false;
                }
                else
                {
                    x = System.Math.Max(x, 0);
                }

                if(clipMax)                
                    x = x.Clamp(0, MaxValue.Value);                

                if(NegativeSlope!=0)                
                    x -= NegativeSlope * negativePart;                
                return x;
            });
        }
    }
}
