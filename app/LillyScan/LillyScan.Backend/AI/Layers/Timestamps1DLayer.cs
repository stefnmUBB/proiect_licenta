using LillyScan.Backend.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.AI.Layers
{
    public abstract class Timestamps1DLayer : Layer
    {
        protected Timestamps1DLayer() { }
        protected Timestamps1DLayer(Shape[] inputShapes, string name = null) : base(inputShapes, name) { }

        protected override void OnValidateInputShapes(Shape[] inputShapes)
        {
            base.OnValidateInputShapes(inputShapes);
            Assert(() => inputShapes.Length == 1);
            Assert(() => inputShapes[0].Length==3);
        }
    }
}
