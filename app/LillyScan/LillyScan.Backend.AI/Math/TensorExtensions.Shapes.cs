using System;
using System.Linq;

namespace LillyScan.Backend.AI.Math
{
    public static partial class TensorExtensions
    {
        public static Tensor Reshape(this Tensor tensor, Shape newShape) 
        {
            if (tensor.IsScalar && newShape.DimensionsCount == 1 && newShape[0] == 1)
                return new Tensor(newShape, tensor.BuffAccessor, 0);
            if (tensor.Shape.ElementsCount != newShape.ElementsCount)
                throw new InvalidOperationException($"Cannot reshape tensor of shape {tensor.Shape} to {newShape}");
            return new Tensor(newShape, tensor.BuffAccessor, 0);
        }

        public static Tensor[] Unstack(this Tensor t, int axis = 0)
        {
            axis = Shape.SolveIndex(t.Rank, axis);
            var accessors = new ISequenceAccessor[t.Rank];

            var tensors = new Tensor[t.Shape[axis]];
            for (int i = 0; i < tensors.Length; i++)
            {
                accessors[axis] = new IndexAccessor(i);
                tensors[i] = t[accessors];
            }
            return tensors;
        }
    }
}
