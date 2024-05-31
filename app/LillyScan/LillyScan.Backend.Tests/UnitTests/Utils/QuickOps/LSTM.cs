using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.Math;
using LillyScan.Backend.Tests.TestTemplates;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace LillyScan.Backend.Tests.UnitTests.Utils.QuickOps
{
    public class LSTM
    {
        private readonly ITestOutputHelper output;

        public LSTM(ITestOutputHelper output)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;            
            this.output = output;
            TestTemplate.Output = output;
        }


        private (float[] c, float[] h) LSTMCellCall(
            (float[] x, float[] c, float[] h, float[] w, float[] u, float[] b) input)
        {
            int L = input.x.Length, U = input.c.Length;
            var tmp = new float[8 * U];
            var rc = new float[U];
            var rh = new float[U];
            Backend.Utils.QuickOps.ForwardLSTMStep(input.c, input.h, input.x, input.w, input.u, input.b, rc, rh, tmp, L, U);
            return (rc, rh);
        }

        private (float[] c, float[] h) LSTMCellCall2(
         (float[] x, float[] c, float[] h, float[] w, float[] u, float[] b) input)
        {
            int L = input.x.Length, U = input.c.Length;
            var cell = new LSTMCell(L, U, useBias: true);
            cell.Build();
            cell.LoadWeights(new[] 
            { 
                new Tensor<float>((L, 4 * U), input.w), 
                new Tensor<float>((U, 4 * U), input.u),
                new Tensor<float>((4*U), input.b)
            });            
            var cc = new Tensor<float>(U, input.c);
            var hh = new Tensor<float>(U, input.h);
            var xx = new Tensor<float>(L, input.x);
            var output = cell.Call(cc, hh, xx);
            return (output[0].Buffer.Buffer, output[1].Buffer.Buffer);
        }


        [Fact]
        public void Test_LSTMCell()
        {
            TestTemplate.RunBatchTestFiles(LSTMCellOpTestTemplate.FromTestFiles, "FileTests\\lstmcell", 3, 3, LSTMCellCall);
        }
    }
}
