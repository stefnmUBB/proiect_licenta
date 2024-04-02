using LillyScan.Backend.AI.Activations;
using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using System;
using System.Collections.Generic;
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

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            var input = inputs[0];            
            var output = input.SubDimMap(t =>
            {
                var cs = new List<Tensor<float>>();
                var c = Tensors.Zeros<float>(Units);
                var h = Tensors.Zeros<float>(Units);

                for (int i = 0; i < t.Shape[0]; i++)
                {
                    var x = t.GetFromBatches(new[] { i });
                    var cellOutput = Cell.Call(c, h, x);
                    c = cellOutput[0];
                    h = cellOutput[1];
                    //c.Print();
                    //h.Print();
                    cs.Add(c);

                }
                return Tensors.Stack(cs);
                //return c;
            }, 2);
            return new[] { output };
        }
        
    }
}
