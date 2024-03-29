using LillyScan.Backend.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.AI.Layers.TfConfigConverters
{
    public static class ShapesConverter
    {
        public static Shape[] Convert(object[] input)
        {
            if (input.All(i => i == null || i is int))
                return new[] { ShapeConverter.Convert(input) };
            if(input.All(i=>i is object[]))
                return input.Select(_=>ShapeConverter.Convert(_ as object[])).ToArray();
            throw new TfConfigConverterFailedException(input, typeof(Shape[]));
        }
    }
}
