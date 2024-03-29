using LillyScan.Backend.Math;
using LillyScan.Backend.Types;

namespace LillyScan.Backend.AI.Layers
{
    [Named("InputLayer")]
    public class InputLayer : Layer
    {
        public InputLayer(Shape[] inputShapes, string name=null) : base(inputShapes, name) { }        

        public InputLayer(Shape inputShape, string name = null) : base(new[] { inputShape }, name) { }        

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            return inputs;
        }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes) => inputShapes;        
    }
}
