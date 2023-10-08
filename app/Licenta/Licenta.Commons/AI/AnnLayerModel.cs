using System;

namespace Licenta.Commons.AI
{
    public class AnnLayerModel
    {
        public Type PerceptronType { get; }
        public int PerceptronsCount { get; set; }
        public AnnLayerModel(Type perceptronType, int perceptronsCount)
        {
            PerceptronType = perceptronType;
            PerceptronsCount = perceptronsCount;
        }
    }
}
