using LillyScan.Backend.AI.Activations;
using LillyScan.Backend.Math;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Types;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.AI.Layers
{
    [Named("LSTM")]
    public class LSTM : Timestamps1DLayer
    {

        internal LSTM() { }

        public LSTM(Shape inputShape, int units,
            Activations.Activation activation = null,
            Activations.Activation recurrentActivation = null,
            bool useBias = false,
            string name = null) : base(new[] { inputShape }, name)
        {
            Units = units;
            Activation = activation ?? new Tanh();
            RecurrentActivation = recurrentActivation ?? new Sigmoid();
            UseBias = useBias;
            Build();
        }

        public int Units { get; private set; }

        public Activations.Activation Activation { get; private set; }

        public Activations.Activation RecurrentActivation { get; private set; }

        public bool UseBias { get; private set; }

        private LSTMCell Cell;

        public override void OnBuild()
        {            
            Cell = new LSTMCell(InputShapes[0][2],
                units: Units, activation: Activation, recurrentActivation: RecurrentActivation,
                useBias: UseBias);
        }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes)
        {
            var inputShape = inputShapes[0].ToArray();
            inputShape[2] = Units;
            return new Shape[] { inputShape };
        }

        static string ToHexString(float f)
        {
            var bytes = BitConverter.GetBytes(f);
            var i = BitConverter.ToInt32(bytes, 0);
            return i.ToString("X8");
        }        
        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {            
            var input = inputs[0];
            (var B, var T, var L) = (input.Shape[0], input.Shape[1], input.Shape[2]);
            var U = Units;
            var outBuf = new float[B * T * U];
            var w = Context.GetWeight("W", (L, 4 * U)).Buffer.Buffer;
            var u = Context.GetWeight("U", (U, 4 * U)).Buffer.Buffer;
            var b = UseBias ? Context.GetWeight("B", (4 * U)).Buffer.Buffer : new float[4 * U];

            QuickOps.ForwardLSTM(input.Buffer.Buffer, outBuf, w, u, b, B, T, L, U);
            return new[] { new Tensor<float>((B, T, U), outBuf) };
            /*var output = input.SubDimMap(t =>
            {
                var hs = new List<Tensor<float>>();
                var c = Tensors.Zeros<float>(Units);
                var h = Tensors.Zeros<float>(Units); 
                
                for (int i = 0; i < t.Shape[0]; i++)
                {                    
                    var x = t.GetFromBatches(new[] { i });                    
                    var cellOutput = Cell.Call(c, h, x);
                    c = cellOutput[0];
                    h = cellOutput[1];
                    hs.Add(h);
                }
                return Tensors.Stack(hs);                
            }, 2);
            return new[] { output };*/
        }

        public override void LoadWeights(Tensor<float>[] weights)
        {
            base.LoadWeights(weights);
            Cell.LoadWeights(weights);

            Assert("LSTM: weights.Length == (UseBias ? 3 : 2)", weights.Length == (UseBias ? 3 : 2));
            //Assert("LSTMCell: WShape.Equals(weights[0].Shape)", new Shape(InputShapes[0]).Equals(weights[0].Shape));
            //Assert("LSTMCell: UShape.Equals(weights[1].Shape)", UShape.Equals(weights[1].Shape));

            Context.Weights["W"] = weights[0];
            Context.Weights["U"] = weights[1];

            if (UseBias)
            {
                //Assert("LSTMCell: BiasShape.Equals(weights[2].Shape)", BiasShape.Equals(weights[2].Shape));
                Context.Weights["B"] = weights[2];
            }
        }

    }
}
