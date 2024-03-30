using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
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
            var i0 = LSTMForward.Call(inputs)[0];
            var i1 = LSTMForward.Call(inputs)[0];
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
            Assert(() => weights.Length == 6);
            LSTMForward.LoadWeights(weights.Take(3).ToArray());
            LSTMBackward.LoadWeights(weights.Skip(3).ToArray());
        }
    }
}
