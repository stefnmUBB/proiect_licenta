using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.AI.Layers
{
    [Named("LeakyReLU")]
    public class LeakyReLU : Layer
    {
        [TfConfigProperty("alpha")]
        public float Alpha { get; private set; } = 0.01f;

        private Activations.Activation ActivationFunction = null;

        internal LeakyReLU() { }
        public LeakyReLU(Shape[] inputShapes, float alpha, string name = null) : base(inputShapes, name)
        {
            Alpha = alpha;
        }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes) => inputShapes;

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            if (ActivationFunction == null)
                ActivationFunction = new Activations.LeakyReLU(Alpha);
            return inputs.Select(ActivationFunction.Call).ToArray();
        }
    }
}
