using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.AI.Layers
{

    [Named("Softmax")]
    public class Softmax : Layer
    {        
        private readonly Activations.Activation ActivationFunction = new AI.Activations.Softmax();

        internal Softmax() { }
        public Softmax(Shape[] inputShapes, string name = null) : base(inputShapes, name) { }        

        public override Shape[] OnGetOutputShape(Shape[] inputShapes) => inputShapes;

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {            
            return inputs.Select(ActivationFunction.Call).ToArray();
        }
    }
}
