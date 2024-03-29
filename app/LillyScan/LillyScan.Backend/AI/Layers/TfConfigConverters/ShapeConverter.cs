using LillyScan.Backend.Math;
using System;
using System.Linq;

namespace LillyScan.Backend.AI.Layers.TfConfigConverters
{
    public static class ShapeConverter
    {
        public static Shape Convert(int[] input) => new Shape(input);
        public static Shape Convert(object[] input)
        {
            var dims = input
                    .Select(_ => _ is int d ? d : _ == null ? -1 : throw new InvalidOperationException($"Invalid dimension: {_}"))
                    .ToArray();
            return new Shape(dims);
        }
    }
}
