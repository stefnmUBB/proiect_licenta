using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using System;

namespace LillyScan.Backend.AI.Activations
{
    public abstract class Activation
    {
        public abstract Tensor<float> Call(Tensor<float> input);

        public static implicit operator Activation(string activationName)
        {            
            return NameSolver.GetType(activationName, typeof(Activation)) is Type type
                ? Activator.CreateInstance(type) as Activation
                : throw new InvalidOperationException($"No activation function named `{activationName}`");
        }
    }
}
