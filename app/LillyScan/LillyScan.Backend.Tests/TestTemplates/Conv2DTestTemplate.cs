using LillyScan.Backend.Math;
using LillyScan.Backend.Tests.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LillyScan.Backend.Tests.TestTemplates
{
    class Conv2DTestTemplate : TestTemplate<(Tensor<float> Source, Tensor<float> Kernel), Tensor<float>>
    { 
        public Conv2DTestTemplate(IOTestData testData) : base(testData) { }

        protected override (Tensor<float> Source, Tensor<float> Kernel) DecodeInput()
        {
            var lines = TestData.Input.Split("\n");
            var dims = lines[0].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            int b = dims[0], n = dims[1], m = dims[2], c = dims[3], k1 = dims[4], k2 = dims[5], f = dims[6];
            var src = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(float.Parse).ToArray();
            var ker = lines[2].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(float.Parse).ToArray();
            return (
                new Tensor<float>((b, n, m, c), src),
                new Tensor<float>((k1, k2, c, f), ker)
            );
        }

        protected override Tensor<float> DecodeOutput()
        {
            var lines = TestData.Output.Split("\n");
            var dims = lines[0].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            var output = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(float.Parse).ToArray();
            int b = dims[0], n = dims[1], m = dims[2], f = dims[3];
            return new Tensor<float>((b, n, m, f), output);
        }

        protected override bool CompareOutputs(Tensor<float> o1, Tensor<float> o2)
        {          
            return o1.ApproxEquals(o2);            
        }        

        public static Conv2DTestTemplate FromTestFiles(string name) => new Conv2DTestTemplate(IOTestData.FromTestFiles(name));         
    }
}
