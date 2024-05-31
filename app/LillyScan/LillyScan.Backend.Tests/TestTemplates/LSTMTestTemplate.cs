using LillyScan.Backend.Math;
using LillyScan.Backend.Tests.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LillyScan.Backend.Tests.TestTemplates
{
    internal class LSTMTestTemplate : TestTemplate<(Tensor<float> x, Tensor<float> w, Tensor<float> u, Tensor<float> b), Tensor<float>>
    {
        public LSTMTestTemplate(IOTestData testData) : base(testData) { }        

        protected override bool CompareOutputs(Tensor<float> o1, Tensor<float> o2) => o1.ApproxEquals(o2);                    

        protected override (Tensor<float> x, Tensor<float> w, Tensor<float> u, Tensor<float> b) DecodeInput()
        {
            var lines = TestData.Input.Split("\n");
            var pms = TestTemplate.ReadInts(lines[0]);
            (var B, var T, var L, var U, var ub) = (pms[0], pms[1], pms[2], pms[3], pms[4]);

            var x = new Tensor<float>((B, T, L), TestTemplate.ReadFloats(lines[1]));
            var w = new Tensor<float>((L, 4*U), TestTemplate.ReadFloats(lines[2]));
            var u = new Tensor<float>((U, 4*U), TestTemplate.ReadFloats(lines[3]));            
            var b = ub != 0 ? new Tensor<float>((4 * U), TestTemplate.ReadFloats(lines[4])) : Tensors.Zeros<float>((4 * U));

            TestTemplate.Output?.WriteLine($"b={string.Join(", ", b.Buffer)}");

            return (x, w, u, b);
        }

        protected override Tensor<float> DecodeOutput()
        {
            var lines = TestData.Output.Split("\n");
            var pms = TestTemplate.ReadInts(lines[0]);
            (var B, var T, var U) = (pms[0], pms[1], pms[2]);
            var y = new Tensor<float>((B, T, U), TestTemplate.ReadFloats(lines[1]));
            return y;
        }

        protected override string OutputToString(Tensor<float> o)
        {
            return string.Join(" ", o.Buffer);
        }

        public static LSTMTestTemplate FromTestFiles(string name) => new LSTMTestTemplate(IOTestData.FromTestFiles(name));
    }
}
