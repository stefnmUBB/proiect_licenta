using LillyScan.Backend.Math;
using LillyScan.Backend.Types;

namespace LillyScan.Backend.AI.Activations
{
    [Named("softmax")]
    public class Softmax : Activation
    {
        public override unsafe Tensor<float> Call(Tensor<float> input)
        {
            int elemsCount = input.Shape.ElementsCount;
            var buffer = new float[elemsCount];

            int logitsLen = input.Shape[-1];
                        


            fixed(float* ibuff = &input.Buffer.Buffer[0])
            fixed(float* obuff = &buffer[0])
            {
                for(int i=0;i<elemsCount;i+=logitsLen)
                {
                    float max = 0;
                    /*float max = ibuff[0];
                    for (int j = 1; j < logitsLen; j++)
                        if (ibuff[i + j] > max) max = ibuff[i + j];*/

                    double sum = 0;
                    for(int j=0;j<logitsLen;j++)
                    {
                        obuff[i + j] = (float)System.Math.Exp(ibuff[i + j] - max);
                        sum += obuff[i + j] - max;
                    }

                    for (int j = 0; j < logitsLen; j++)
                    {
                        obuff[i + j] = (float)(obuff[i + j] / sum);
                    }
                }
            }

            return new Tensor<float>(input.Shape, buffer);
        }
    }
}
