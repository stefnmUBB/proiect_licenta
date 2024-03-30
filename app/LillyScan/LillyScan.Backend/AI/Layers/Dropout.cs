using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.AI.Layers
{
    [Named("Dropout")]
    internal class Dropout : Layer
    {
        internal Dropout() { }

        [TfConfigProperty("rate")]
        public double Rate { get; private set; }

        public Dropout(Shape[] inputShapes, double rate=0.2, string name=null):base(inputShapes, name) 
        {
            Rate = rate;
        }
        public Dropout(Shape inputShape, double rate=0.2, string name = null) : this(new[] {inputShape}, rate, name) { }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes)
        {
            return inputShapes;
        }

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            // deactivate in prediction
            return inputs;
        }
    }
}
