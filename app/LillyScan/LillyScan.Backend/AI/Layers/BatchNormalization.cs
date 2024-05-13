using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.AI.Layers
{
    [Named("BatchNormalization")]
    public class BatchNormalization : Layer
    {
        internal BatchNormalization() { }

        [TfConfigProperty("momentum")]
        public float Momentum { get; private set; }

        [TfConfigProperty("epsilon")]
        public float Epsilon { get; private set; }

        // assume 'center': True, 'scale': True

        public BatchNormalization(Shape[] inputShapes, float momentum, float epsilon, string name = null) : base(inputShapes, name)
        {
            (Momentum, Epsilon) = (momentum, epsilon);
        }


        public override Shape[] OnGetOutputShape(Shape[] inputShapes) => inputShapes;

        protected override void OnValidateInputShapes(Shape[] inputShapes)
        {
            base.OnValidateInputShapes(inputShapes);
            Assert("BatchNormalization: invalid inputs count", inputShapes.Length == 1);
        }        

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            //return inputs;
            var gamma = Context.Weights["gamma"];
            var beta = Context.Weights["beta"];
            var movingMean = Context.Weights["moving_mean"];
            var movingVariance = Context.Weights["moving_variance"];
            var input = inputs[0];            
            var nom = movingVariance.FastFloatAdd(Tensors.Constant(new Shape(), Epsilon));
            var result = input.FastFloatSub(movingMean).FastFloatDiv(nom.Sqrt()).FastFloatMul(gamma).FastFloatAdd(beta);
            return new[] { result };
        }
        

        public override void LoadWeights(Tensor<float>[] weights)
        {
            Context.Weights["gamma"] = weights[0];
            Context.Weights["beta"] = weights[1];
            Context.Weights["moving_mean"] = weights[2];
            Context.Weights["moving_variance"] = weights[3];
            
        }
    }
}
