using System;
using System.Linq;

namespace Licenta.Commons.AI
{
    public static class LossFunctions
    {
        public static double MeanSquaredError(double[] r, double[] p)
        {
            return r.Length == 0 ? 0 : r.Zip(p, (a, b) => (a - b) * (a - b)).Sum() / r.Length;
        }

        public static double SquaredError(double[] r, double[] p)        
        {
            return r.Length == 0 ? 0 : r.Zip(p, (a, b) => (a - b) * (a - b)).Sum();
        }

        public static double LogLoss(double[] r, double[] p)        
        {
            double loss = 0;
            for (int k = 0; k < r.Length; k++)
            {
                if (r[k] != 0)
                    loss -= r[k] * Math.Log(p[k]);
            }
            return loss;
        }
    }
}
