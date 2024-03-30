using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.AI.Layers
{
    [Named("MaxPooling2D")]
    public class MaxPooling2D : Pooling2D
    {
        internal MaxPooling2D() : base(MaxPooling) { }
        public MaxPooling2D(Shape[] input_shapes, (int, int) poolSize, (int, int)? strides = null, Padding padding = Padding.Valid, string name = null)
            : base(MaxPooling, input_shapes, poolSize, strides, padding, name) { }

        public MaxPooling2D(Shape input_shape, (int, int) poolSize, (int, int)? strides = null, Padding padding = Padding.Valid, string name = null)
            : base(MaxPooling, new[] {input_shape}, poolSize, strides, padding, name) { }

        private static float MaxPooling(float[] values) => values.Max();        
    }
}
