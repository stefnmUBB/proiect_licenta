using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LillyScan.Backend.AI.Layers
{
    [Named("Reshape")]
    public class Reshape : Layer
    {
        [TfConfigProperty("target_shape",converter:typeof(ShapeConverter))]
        public Shape TargetShape { get; private set; }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes)
        {
            return new[] { TargetShape };
        }

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            return new[] { inputs[0].Reshape(TargetShape) };
        }

        protected override void OnValidateInputShapes(Shape[] inputShapes)
        {
            base.OnValidateInputShapes(inputShapes);
            Assert(() => inputShapes.Length == 1, () => inputShapes[0].ElementsCount == TargetShape.ElementsCount);            
        }
    }
}
