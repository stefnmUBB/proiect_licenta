using LillyScan.Backend.AI.Models;
using LillyScan.Backend.Math;
using LillyScan.Backend.Properties;
using System;
using System.Diagnostics;
using System.Linq;

namespace LillyScan.Backend.API
{
    public class DefaultHTREngine : HTREngine
    {
        private readonly Model SegmentationModel;
        private readonly Model SegmentationModel64;

        public DefaultHTREngine(Model segmentationModel = null, Model segmentationModel64 = null)
        {
            SegmentationModel = segmentationModel ?? ModelLoader.LoadFromBytes(Resources.seg_model);
            SegmentationModel64 = segmentationModel64 ?? ModelLoader.LoadFromBytes(Resources.q_seg_model_txt_lsm);
            Debug.WriteLine("Loaded DefaultHTREngine");
        }

        public override string Predict(IReadMatrix<double> image)
        {
            throw new NotImplementedException();
        }

        public override float[] Segment(float[] image)
        {            
            var predicted = SegmentationModel.Call(new[] { new Tensor<float>((1, 256, 256, 1), image) })[0];
            return predicted.Buffer.Buffer.ToArray();
        }

        public override float[] Segment64(float[] image)
        {
            var predicted = SegmentationModel64.Call(new[] { new Tensor<float>((1, 64, 64, 1), image) })[0];
            return predicted.Buffer.Buffer.ToArray();
        }
    }
}
