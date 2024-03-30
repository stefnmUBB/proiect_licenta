using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.AI.Activations
{
    [Named("tanh")]
    internal class Tanh : Activation
    {                        
        public override Tensor<float> Call(Tensor<float> input)
        {
            return input.Map(x => (float)System.Math.Tanh(x));
        }
    }
}
