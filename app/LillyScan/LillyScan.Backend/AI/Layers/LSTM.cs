using LillyScan.Backend.AI.Activations;
using LillyScan.Backend.Math;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Types;
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

        static int K = 0;
        static int Q = 0;
        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            using (var f = File.CreateText($"D:\\anu3\\proiect_licenta\\app\\LillyScan\\LillyScan\\bin\\Debug\\cc\\v_{Q++}.txt")) 
            {
                var w = Cell.Context.Weights["W"].Buffer.Buffer;
                var u = Cell.Context.Weights["U"].Buffer.Buffer;
                var b = Cell.Context.Weights["B"].Buffer.Buffer;
                f.WriteLine($"# {Cell.Context.Weights["W"].Shape}");
                //f.WriteLine($"W=[{w.JoinToString(", ")}]");
                f.WriteLine($"W=[{w.Select(_=>$"\"{ToHexString(_)}\"").JoinToString(", ")}]");
                f.WriteLine($"# {Cell.Context.Weights["U"].Shape}");
                //f.WriteLine($"U=[{u.JoinToString(", ")}]");
                f.WriteLine($"U=[{u.Select(_ => $"\"{ToHexString(_)}\"").JoinToString(", ")}]");
                f.WriteLine($"# {Cell.Context.Weights["B"].Shape}");
                //f.WriteLine($"B=[{b.JoinToString(", ")}]");
                f.WriteLine($"B=[{b.Select(_ => $"\"{ToHexString(_)}\"").JoinToString(", ")}]");
            }

            var input = inputs[0];            
            var output = input.SubDimMap(t =>
            {
                var hs = new List<Tensor<float>>();
                var c = Tensors.Zeros<float>(Units);
                var h = Tensors.Zeros<float>(Units); 
                
                for (int i = 0; i < t.Shape[0]; i++)
                {
                    //c = Tensors.Zeros<float>(Units);
                    //h = Tensors.Zeros<float>(Units);
                    var x = t.GetFromBatches(new[] { i });
                    using (var f = File.CreateText($@"D:\anu3\proiect_licenta\app\LillyScan\LillyScan\bin\Debug\cc\D\{K++}.txt"))
                    {
                        x.Print($"Input {i}", f);
                    }                    
                    var cellOutput = Cell.Call(c, h, x);
                    c = cellOutput[0];
                    h = cellOutput[1];
                    hs.Add(h);
                }
                return Tensors.Stack(hs);                
            }, 2);
            return new[] { output };
        }

        public override void LoadWeights(Tensor<float>[] weights)
        {
            base.LoadWeights(weights);
            Cell.LoadWeights(weights);
        }

    }
}
