using LillyScan.Backend.Math;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Utils;
using System.IO;
using System.Linq;

namespace LillyScan.Backend.AI.Layers
{
    internal class LayerInfo
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public TfConfig Config { get; set; }        
        public Shape[] WeightShapes { get; set; } = new Shape[0];
        public Tensor<float>[] Weights { get; set; }
        public LayerInfo[] Inputs { get; set; }

        public override string ToString() => $"LayerInfo(type={Type};" +
            $"name={Name};" +
            $"inputs={Inputs?.Select(_ => _.Name)?.JoinToString(",")};" +
            $"weight_shapes={WeightShapes.JoinToString(",")};" +
            $"output={IsOutput})";

        public bool IsOutput { get; set; } = false;

        public Layer ToLayer(Shape[] inputShapes)
        {
            return LayerDecoder.Decode(Type, inputShapes, Name, Config, Weights);
        }                
    }
}
