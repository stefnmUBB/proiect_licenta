using LillyScan.Backend.AI.Activations;
using LillyScan.Backend.Math;
using LillyScan.Backend.Utils;
using System;
using System.IO;
using System.Linq;

namespace LillyScan.Backend.AI.Layers
{
    public class LSTMCell : Layer
    {        
        public int Units { get; private set; }
        
        public Activations.Activation Activation { get; private set; }
       
        public Activations.Activation RecurrentActivation { get; private set; }
        
        public bool UseBias { get; private set; }

        Shape HidddenShape => (Units);
        Shape WShape => (InputShapes[2][0], 4*Units);
        Shape UShape => (Units, 4*Units);
        Shape BiasShape => (4*Units);

        public LSTMCell(Shape inputShape, int units, 
            Activations.Activation activation = null, 
            Activations.Activation recurrentActivation = null,            
            bool useBias = false,
            string name = null) : base(new[] { units, units, inputShape }, name)
        {
            Units = units;
            Activation = activation ?? new Tanh();
            RecurrentActivation = recurrentActivation ?? new Sigmoid();
            UseBias = useBias;

            Context.Weights["W"] = Tensors.Ones<float>(WShape);
            Context.Weights["U"] = Tensors.Ones<float>(UShape);
            if (useBias)
                Context.Weights["bias"] = Tensors.Ones<float>(BiasShape);
        }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes) => new[] { HidddenShape, HidddenShape }; // c,h                

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            (var c, var h, var x) = (inputs[0], inputs[1], inputs[2]);
            c = c.Reshape(1 + c.Shape);
            h = h.Reshape(1 + h.Shape);
            x = x.Reshape(1 + x.Shape);         

            var W = Context.GetWeight("W");
            var U = Context.GetWeight("U");
            var t = x.MatMul(W).Add(h.MatMul(U));            
            
            if (UseBias) t = t.Add(Context.GetWeight("B"));            
            t = t.Reshape((t.Shape[0], 4, Units));
            var it = RecurrentActivation.Call(t[null, new IndexAccessor(0)]);
            var ft = RecurrentActivation.Call(t[null, new IndexAccessor(1)]);
            var ctt = Activation.Call(t[null, new IndexAccessor(2)]);
            var ot = RecurrentActivation.Call(t[null, new IndexAccessor(3)]);            
            var ct = ft.Multiply(c).Add(it.Multiply(ctt));            
            var ht = ot.Multiply(Activation.Call(ct));            

            return new[] { ct.Reshape(Units), ht.Reshape(Units) };
        }

        protected override void OnValidateInputShapes(Shape[] inputShapes)
        {
            base.OnValidateInputShapes(inputShapes);
            Assert("LSTMCell: InputShapes.Length == 3", InputShapes.Length == 3);
            (var h, var c, var x) = (inputShapes[0], inputShapes[1], inputShapes[2]);
            Assert("LSTMCell: h.Length == 1 && c.Length == 1 && x.Length == 1", h.Length == 1, c.Length == 1, x.Length == 1);
            Assert("LSTMCell: h.SequenceEqual(c)", h.SequenceEqual(c));
        }

        public override void LoadWeights(Tensor<float>[] weights) 
        {
            Assert("LSTMCell: weights.Length == (UseBias ? 3 : 2)", weights.Length == (UseBias ? 3 : 2));
            Assert("LSTMCell: WShape.Equals(weights[0].Shape)", WShape.Equals(weights[0].Shape));
            Assert("LSTMCell: UShape.Equals(weights[1].Shape)", UShape.Equals(weights[1].Shape));

            Context.Weights["W"] = weights[0];
            Context.Weights["U"] = weights[1];

            if (UseBias)
            {
                Assert("LSTMCell: BiasShape.Equals(weights[2].Shape)", BiasShape.Equals(weights[2].Shape));
                Context.Weights["B"] = weights[2];
            }
        }
    }
}
