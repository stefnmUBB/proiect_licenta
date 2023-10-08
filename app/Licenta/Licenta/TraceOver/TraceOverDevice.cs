using HelpersCurveDetectorDataSetGenerator.Commons.Math.Arithmetics;
using HelpersCurveDetectorDataSetGenerator.Commons.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpersCurveDetectorDataSetGenerator.TraceOver
{
    using DoubleMatrix = Matrix<DoubleNumber>;

    public class TraceOverDevice
    {
        public DoubleMatrix Source { get; }

        public TraceOverDevice(DoubleMatrix source)
        {
            Source = source;
        }



    }
}
