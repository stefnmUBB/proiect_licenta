using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.Math
{
    public static class Shapes
    {
        public static bool CanConcatenate(IEnumerable<Shape> shapes, int axis)
        {
            var inputShapes = shapes.ToArray();
            if (inputShapes.Select(_ => _.Length).Distinct().Count() != 1)
                throw new ArgumentException("All shapes must have the same rank");
            var rank = inputShapes[0].DimsCount;
            axis = Shape.ResolveIndex(rank, axis);
            for (int i = 0; i < rank; i++)
            {
                if (i == axis) continue;
                if (shapes.Select(_ => _[i]).Distinct().Count() != 1)                
                    return false; 
            }
            return true;
        }

        public static void ValidateConcatenate(IEnumerable<Shape> shapes, int axis)
        {
            if (!CanConcatenate(shapes, axis))
                throw new ArgumentException($"Cannot concatenate tensors of shapes: {shapes.JoinToString(", ")}" +
                    $"along axis {axis}");
        }

        public static Shape GetConcatenatedShape(IEnumerable<Shape> shapes, int axis, bool skipValidation=false)
        {
            if (!skipValidation)
                ValidateConcatenate(shapes, axis);
            var inputShapes = shapes.ToArray();            
            var dims = inputShapes[0].ToArray();
            var rank = inputShapes[0].DimsCount;
            axis = Shape.ResolveIndex(rank, axis);
            dims[axis] = shapes.Sum(_ => _[axis]);
            return new Shape(dims);

        }
    }
}
