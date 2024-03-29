using LillyScan.Backend.Math;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Types;
using System;

namespace LillyScan.Backend.AI.Layers
{
    [Named("Concatenate")]
    public class Concatenate : Layer
    {
        [TfConfigProperty("axis")]
        public int Axis { get; private set; } = -1;

        internal Concatenate() { }

        public Concatenate(Shape[] inputShapes, int axis=-1, string name=null) : base(inputShapes, name)
        {
            Axis = axis;
        }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes)
        {
            return new[] { Shapes.GetConcatenatedShape(inputShapes, Axis, skipValidation: true) };
        }

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            return new[] { Tensors.Concatenate(inputs, Axis) };
        }

        protected override void OnValidateInputShapes(Shape[] inputShapes)
        {
            if (!Shapes.CanConcatenate(inputShapes, Axis))
                throw new ArgumentException(
                    $"Cannot concatenate tensors of shapes: {inputShapes.JoinToString(", ")} along axis {Axis}");
        }     
    }
}
