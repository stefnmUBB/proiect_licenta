using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.AI.Math
{
    public static partial class TensorExtensions
    {
        private static Tensor ReduceOneAxis(this Tensor t, Func<float, float, float> op, int axis, bool keepDimensions)
        {
            axis = Shape.SolveIndex(t.Rank, axis);
            return t.SubDimMap(x =>
            {
                var tensors = x.Unstack(axis: 0);
                var r = tensors[0];
                for (int i = 1; i < tensors.Length; i++)
                    r = r.PerformElementWiseBinaryOperation(tensors[i], op);
                if (keepDimensions)
                    r = r.Reshape(1 + r.Shape);
                return r;
            }, t.Rank - axis);
        }

        public static Tensor ReduceAxis(this Tensor t, Func<float, float, float> op, AxisCollection axis = null, bool keepDimensions = false)
        {
            axis = axis ?? AxisCollection.AllAxis;
            var axisArr = axis.Resolve(t.Rank);
            for (int i = axisArr.Length - 1; i >= 0; i--)
                t = t.ReduceOneAxis(op, axisArr[i], keepDimensions);
            return t;
        }


        public static Tensor ReduceSum(this Tensor t, AxisCollection axis = null, bool keepDimensions = false)
        {
            return t.ReduceAxis((x, y) => x + y, axis, keepDimensions);
        }
    }
}
