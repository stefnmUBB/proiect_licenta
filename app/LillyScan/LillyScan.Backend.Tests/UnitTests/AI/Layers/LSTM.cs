using LillyScan.Backend.Math;
using LillyScan.Backend.Tests.TestTemplates;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace LillyScan.Backend.Tests.UnitTests.AI.Layers
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

        private Tensor<float> LSTMCall(
          (Tensor<float> x, Tensor<float> w, Tensor<float> u, Tensor<float> b) input)
        {
            (var B, var T, var L) = (input.x.Shape[0], input.x.Shape[1], input.x.Shape[2]);
            var U = input.u.Shape[0];
            var lstm = new Backend.AI.Layers.LSTM((B, T, L), U, useBias: true);
            lstm.Build();
            lstm.LoadWeights(new[] { input.w, input.u, input.b });

            /*lstm.Context.Weights["B"] = input.b;
            lstm.Context.Weights["W"] = input.w;
            lstm.Context.Weights["U"] = input.u;                        */
            var y = lstm.Call(input.x)[0];
            return y;
        }

        private Tensor<float> LSTMCallUnsafe(
         (Tensor<float> x, Tensor<float> w, Tensor<float> u, Tensor<float> b) input)
        {
            (var B, var T, var L) = (input.x.Shape[0], input.x.Shape[1], input.x.Shape[2]);
            var U = input.u.Shape[0];
            var outBuf = new float[B * T * U];
            QuickOps.ForwardLSTM(input.x.Buffer.Buffer, outBuf, input.w.Buffer.Buffer, input.u.Buffer.Buffer, input.b.Buffer.Buffer, B, T, L, U);                            
            return new Tensor<float>((B, T, U), outBuf);
        }

        [Fact]
        public void Test_LSTM()
        {
            TestTemplate.RunBatchTestFiles(LSTMTestTemplate.FromTestFiles, "FileTests\\lstm", 1, 1, LSTMCall);
        }
    }
}
