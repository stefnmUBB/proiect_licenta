using Licenta.Commons.AI.Perceptrons;
using System.Collections.Generic;

namespace Licenta.Commons.AI
{
    public class AnnModel
    {
        public List<AnnLayerModel> HiddenLayers { get; } = new List<AnnLayerModel>();        
        public int InputLength { get; set; }
        public int OutputLength { get; set; }

        public RuntimeAnn Compile() => RuntimeAnn.CompileFromModel(this);        
    }
}
