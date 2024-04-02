using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;
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
            return new[] { inputShapes[0][0] + TargetShape };
        }

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            Console.WriteLine("Reshape in: " + inputs.SelectShapes().JoinToString(", "));
            var output = inputs[0].SubDimMap(x => x.Reshape(TargetShape), inputs[0].Rank - 1);
            Console.WriteLine("Reshape out: " + output.Shape);
            return new[] { output };
        }

        protected override void OnValidateInputShapes(Shape[] inputShapes)
        {
            base.OnValidateInputShapes(inputShapes);
            var itemShape = new Shape(inputShapes[0].Skip(1).ToArray());
            Assert(() => inputShapes.Length == 1, () => itemShape.ElementsCount == TargetShape.ElementsCount);
        }
    }
}
