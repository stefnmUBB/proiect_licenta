using LillyScan.Backend.Math;
using LillyScan.Backend.Tests.IO;

namespace LillyScan.Backend.Tests.TestTemplates
{
    internal class LSTMCellOpTestTemplate : TestTemplate<(float[] x, float[] c, float[] h, float[] w, float[] u, float[] b), (float[] c, float[] h)>
    {
        public LSTMCellOpTestTemplate(IOTestData testData) : base(testData) { }        

        protected override bool CompareOutputs((float[] c, float[] h) o1, (float[] c, float[] h) o2)
        {
            return o1.c.Zip(o2.c, (a, b) => System.Math.Abs(a - b)).All(_ => _ < 1e-4)
                && o1.h.Zip(o2.h, (a, b) => System.Math.Abs(a - b)).All(_ => _ < 1e-4);
        }

        protected override (float[] x, float[] c, float[] h, float[] w, float[] u, float[] b) DecodeInput()
        {
            var lines = TestData.Input.Split("\n");
            var ints = TestTemplate.ReadInts(lines[0]);
            int L = ints[0], U = ints[1], useBias = ints[2];
            var x = TestTemplate.ReadFloats(lines[1]);
            var h = TestTemplate.ReadFloats(lines[2]);
            var c = TestTemplate.ReadFloats(lines[3]);
            var w = TestTemplate.ReadFloats(lines[4]);
            var u = TestTemplate.ReadFloats(lines[5]);
            var b = useBias != 0 ? TestTemplate.ReadFloats(lines[6]) : new float[U];
            TestTemplate.Output?.WriteLine(string.Join(", ", x));
            TestTemplate.Output?.WriteLine(string.Join(", ", h));
            return (x, c, h, w, u, b);
        }

        protected override (float[] c, float[] h) DecodeOutput()
        {
            var lines = TestData.Output.Split("\n");
            var h = TestTemplate.ReadFloats(lines[1]);
            var c = TestTemplate.ReadFloats(lines[2]);
            return (c, h);
        }

        public static LSTMCellOpTestTemplate FromTestFiles(string name) => new LSTMCellOpTestTemplate(IOTestData.FromTestFiles(name));

        protected override string OutputToString((float[] c, float[] h) o)
            => $"({string.Join(", ", o.c.Select(_ => _.ToString()))}; {string.Join(", ", o.h.Select(_ => _.ToString()))})";
    }
}
