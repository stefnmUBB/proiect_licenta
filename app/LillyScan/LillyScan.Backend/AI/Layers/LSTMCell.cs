using LillyScan.Backend.AI.Activations;
using LillyScan.Backend.Math;
using LillyScan.Backend.Utils;
using System;
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
        Shape WShape => (InputShapes[0][0], 4*Units);
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
            h = c.Reshape(1 + h.Shape);
            x = x.Reshape(1 + x.Shape);

            var W = Context.GetWeight("W");
            var U = Context.GetWeight("U");

            var t = x.MatMul(W).Add(h.MatMul(U));
            if (UseBias)
                t = t.Add(Context.GetWeight("bias"));

            var ft = RecurrentActivation.Call(t[null, new IndexAccessor(0)]);
            var it = RecurrentActivation.Call(t[null, new IndexAccessor(1)]);
            var ot = RecurrentActivation.Call(t[null, new IndexAccessor(2)]);
            var ctt = Activation.Call(t[null, new IndexAccessor(3)]);

            var ct = ft.Multiply(c).Add(it.Multiply(ctt));
            ct = Activation.Call(ct);
            var ht = ot.Multiply(ct);

            return new[] { ct.Reshape(Units), ht.Reshape(Units) };
        }

        protected override void OnValidateInputShapes(Shape[] inputShapes)
        {
            base.OnValidateInputShapes(inputShapes);
            Assert(() => InputShapes.Length == 3);
            (var h, var c, var x) = (inputShapes[0], inputShapes[1], inputShapes[2]);
            Assert(() => h.Length == 1, () => c.Length == 1, () => x.Length == 1);            
            Assert(() => h.SequenceEqual(c));
        }

        public override void LoadWeights(Tensor<float>[] weights) 
        {
            Assert(() => weights.Length == 2);
            Assert(() => WShape.Equals(weights[0].Shape));
            Assert(() => UShape.Equals(weights[1].Shape));

            Context.Weights["W"] = weights[0];
            Context.Weights["U"] = weights[1];

            if (UseBias)
            {
                Assert(() => BiasShape.Equals(weights[2].Shape));
                Context.Weights["B"] = weights[2];
            }
        }
    }
}
