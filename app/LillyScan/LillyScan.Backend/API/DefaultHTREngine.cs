using LillyScan.Backend.AI.Models;
using LillyScan.Backend.Math;
using System;
using System.Linq;

namespace LillyScan.Backend.API
{
    public class DefaultHTREngine : HTREngine
    {
        private readonly Model SegmentationModel = ModelLoader.LoadFromString(Properties.Resources.model_segm);

        public override string Predict(IReadMatrix<double> image)
        {
            throw new NotImplementedException();
        }

        public override float[] Segment(float[] image)
        {            
            var predicted = SegmentationModel.Call(new[] { new Tensor<float>((1, 256, 256, 1), image) })[0];
            return predicted.Buffer.Buffer.ToArray();
        }
    }
}
