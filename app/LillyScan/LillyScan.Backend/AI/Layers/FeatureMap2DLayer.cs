using LillyScan.Backend.Math;

namespace LillyScan.Backend.AI.Layers
{
    public abstract class FeatureMap2DLayer : Layer
    {
        internal FeatureMap2DLayer() { }
        protected FeatureMap2DLayer(Shape[] inputShapes, string name="") : base(inputShapes, name) { }

        protected override void OnValidateInputShapes(Shape[] inputShapes)
        {
            base.OnValidateInputShapes(inputShapes);
            Assert("Invalid 2D Feature map shape", inputShapes.Length == 1, inputShapes[0].Length == 4);
        }
    }
}
