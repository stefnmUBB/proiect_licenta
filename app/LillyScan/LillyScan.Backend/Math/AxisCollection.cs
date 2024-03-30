using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.Math
{
    public sealed class AxisCollection
    {
        public int[] Axis { get; }

        public AxisCollection(int[] axis = null)
        {
            Axis = axis;
        }

        public int[] Resolve(int rank)
        {
            if (Axis == null) return Enumerable.Range(0, rank).ToArray();
            return Axis.OrderBy(_ => _).Distinct().Select(a => Shape.ResolveIndex(rank, a)).ToArray();
        }

        public bool[] ResolveMask(int rank)
        {            
            var mask = new bool[rank];
            foreach (var a in Resolve(rank))
                mask[a] = true;
            return mask;
        }


        public static implicit operator AxisCollection(int[] axis) => new AxisCollection(axis.ToArray());
        public static implicit operator AxisCollection(int axis) => new AxisCollection(new[] { axis });

        public static AxisCollection AllAxis => new AxisCollection(null);
    }
}
