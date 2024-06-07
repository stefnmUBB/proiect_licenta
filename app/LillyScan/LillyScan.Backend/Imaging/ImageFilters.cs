using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Imaging
{
    public static class ImageFilters
    {
        public static readonly float[] BoxBlur3x3 = new float[9] { 1f / 9, 1f / 9, 1f / 9, 1f / 9, 1f / 9, 1f / 9, 1f / 9, 1f / 9, 1f / 9 };
    }
}
