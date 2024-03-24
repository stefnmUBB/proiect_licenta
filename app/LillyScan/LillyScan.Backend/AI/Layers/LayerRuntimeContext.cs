using LillyScan.Backend.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.AI.Layers
{
    public class LayerRuntimeContext
    {
        public Dictionary<string, Tensor<float>> Weights { get; } = new Dictionary<string, Tensor<float>>();

        public Tensor<float> GetWeight(string name, Shape expectedShape=null, bool throwWhenNotFound=true)
        {
            if (!Weights.ContainsKey(name))
                throw new KeyNotFoundException($"Cannot find weight `{name}` in the current context");
            var tensor = Weights[name];
            if (expectedShape != null && !object.Equals(tensor.Shape, expectedShape))
                if (throwWhenNotFound)
                    throw new InvalidOperationException($"Weight `{name}` has shape {tensor.Shape}, expected {expectedShape}");
                else return Tensors.Zeros<float>(expectedShape);
            return tensor;
        }

    }
}
