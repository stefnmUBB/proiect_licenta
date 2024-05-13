using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Types;
using System;
using System.Linq;

namespace LillyScan.Backend.AI.Layers
{
    [Named("Bidirectional")]
    public class Bidirectional : Timestamps1DLayer
    {
        internal Bidirectional() { }

        public Bidirectional(Func<LSTM> lstm)
        {
            LSTMForward = lstm();
            LSTMBackward = lstm();
        }

        [TfConfigProperty("layer", converter: typeof(LayerConfigConverter))]
        public TfConfig LayerConfig { get; private set; }

        private LSTM LSTMForward;
        private LSTM LSTMBackward;

        public override Shape[] OnGetOutputShape(Shape[] inputShapes)
        {
            var shape = inputShapes[0].ToArray();
            shape[2] = LSTMForward.GetOutputShapes()[0][2] + LSTMBackward.GetOutputShapes()[0][2];
            return new Shape[] { shape };
        }

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            var B = inputs[0].Shape[0];
            var T = inputs[0].Shape[1];
            var F = inputs[0].Shape[2];

            var rev = new float[B * T * F];
            for(int b=0;b<B;b++)
            {
                for(int t=0;t<T;t++)
                    for(int f=0;f<F;f++)
                    {
                        rev[b * T * F + t * F + f] = inputs[0].Buffer[b * T * F + (T - 1 - t) * F + f];
                    }
            }
            var reversed = new Tensor<float>(inputs[0].Shape, rev);

            var i0 = LSTMForward.Call(inputs)[0];
            var i1 = LSTMBackward.Call(reversed)[0];

            var revOut = new float[B * T * F];
            for (int b = 0; b < B; b++)
            {
                for (int t = 0; t < T; t++)
                    for (int f = 0; f < F; f++)
                    {
                        revOut[b * T * F + t * F + f] = i1.Buffer[b * T * F + (T - 1 - t) * F + f];
                    }
            }
            i1 = new Tensor<float>(i1.Shape, revOut);

            return new[] { Tensors.Concatenate(new[] { i0, i1 }, axis: -1) };
        }

        private LSTM DecodeLSTM()
        {
            return new LSTM(InputShapes[0],
                units: LayerConfig.GetValue<int>("units"),
                activation: LayerConfig.GetValue<string>("activation"),
                recurrentActivation: LayerConfig.GetValue<string>("recurrent_activation"),
                useBias: LayerConfig.GetValue<bool>("use_bias"),
                name: LayerConfig.GetValueOrDefault<string>("name", null)
                );
        }

        public override void OnBuild()
        {
            base.OnBuild();
            LSTMForward = DecodeLSTM();
            LSTMBackward = DecodeLSTM();
        }

        public override void LoadWeights(Tensor<float>[] weights)
        {
            Console.WriteLine($"BiLSTM got weights {weights.SelectShapes().JoinToString(", ")}");
            Assert("Invalid weights length", weights.Length == 6);
            LSTMForward.LoadWeights(weights.Take(3).ToArray());
            LSTMBackward.LoadWeights(weights.Skip(3).ToArray());
        }
    }
}
