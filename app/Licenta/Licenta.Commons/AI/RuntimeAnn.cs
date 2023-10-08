using HelpersCurveDetectorDataSetGenerator.Commons.Math;
using HelpersCurveDetectorDataSetGenerator.Commons.Parallelization;
using Licenta.Commons.AI.Perceptrons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Threading;

namespace Licenta.Commons.AI
{
    public class RuntimeAnn
    {
        private readonly Perceptron[][] Layers;
        private readonly Matrix<double>[] Weights;
        private readonly Input[] InputLayer;
        private readonly Output[] OutputLayer;

        private readonly double[][] PerceptronOutputs;

        public List<double[]> ErrW = new List<double[]>();        
        public List<Matrix<double>> DeltaWeights = new List<Matrix<double>>();        

        private RuntimeAnn(Perceptron[][] perceptronLayers, Matrix<double>[] weights)
        {
            Layers = perceptronLayers;
            Weights = weights;
            InputLayer = perceptronLayers.First().Cast<Input>().ToArray();
            OutputLayer = perceptronLayers.Last().Cast<Output>().ToArray();

            PerceptronOutputs = new double[Layers.Length][];
            for (int i = 0; i < Layers.Length; i++)
            {
                PerceptronOutputs[i] = new double[Layers[i].Length];
                ErrW.Add(new double[Layers[i].Length]);
            }
        }        

        private double XW(int layer, int nodeId)
        {
            if (layer == 0) return 0;
            var row = Weights[layer - 1].GetRowArray(nodeId);
            return Weights[layer - 1].GetRowArray(nodeId).Zip(PerceptronOutputs[layer - 1], (x, y) => x * y).Sum();            
        }

        private void ComputeValueOf(int i, int j)
        {
            PerceptronOutputs[i][j] = Layers[i][j].Activate(XW(i, j));
        }

        private void PropagateInformation()
        {
            for (int j = 0; j < InputLayer.Length; j++)
                PerceptronOutputs[0][j] = InputLayer[j].Value;

            /*ParallelForLoop.Run(i =>
            {
                for (int j = 0; j < Layers[i].Length; j++)
                    ComputeValueOf(i, j);
            }, 0, Layers.Length);*/            

            for (int i = 1; i < Layers.Length; i++)
                for (int j = 0; j < Layers[i].Length; j++)
                    ComputeValueOf(i, j);            
        }

        private void PropagateInformation(double[] x)
        {
            SetInput(x);
            PropagateInformation();
        }

        private void PropagateError(double[] x, double[] t, double learningRate = 0.001)
        {
            for (int r = 0; r < OutputLayer.Length; r++) 
            {
                var d = OutputLayer[r].Derivative(XW(Layers.Length - 1, r));
                var del = d * (t[r] - PerceptronOutputs.Last()[r]);
                ErrW[Layers.Length - 1][r] = del;
                //Console.WriteLine($"Err={del}");
            }

            for (int thisLayerId = Layers.Length - 2; thisLayerId >= 0; thisLayerId--)
            {
                var nextLayerId = thisLayerId + 1;
                var thisLayer = Layers[thisLayerId];
                var nextLayer = Layers[nextLayerId];

                var w = Weights[thisLayerId];

                for (int h1 = 0; h1 < thisLayer.Length; h1++)
                {
                    var d = thisLayer[h1].Derivative(XW(thisLayerId, h1));
                    //ErrW[thisLayerId][h1] = d * w.GetRow(h1).ToArray().DotProduct(ErrW[nextLayerId]);
                    ErrW[thisLayerId][h1] = 0;

                    //Debug.WriteLine($"PE W=({w.RowsCount}, {w.ColumnsCount})");
                    //Debug.WriteLine($"PE h1={h1}");
                    //Debug.WriteLine($"PE h2Max={nextLayer.Length}");                    

                    for (int h2 = 0; h2 < nextLayer.Length; h2++)
                    {
                        ErrW[thisLayerId][h1] += d * w[h2, h1] * ErrW[nextLayerId][h2];
                    }

                    for (int h2 = 0; h2 < nextLayer.Length; h2++)
                    {
                        w[h2, h1] += learningRate * ErrW[nextLayerId][h2] * PerceptronOutputs[thisLayerId][h1];
                    }
                }
            }
        }

        private void PropagateErrorBatch(double[] x, double[] t, double learningRate = 0.001)
        {
            for (int r = 0; r < OutputLayer.Length; r++)
            {
                var d = OutputLayer[r].Derivative(XW(Layers.Length - 1, r));
                var del = d * (t[r] - PerceptronOutputs.Last()[r]);
                ErrW[Layers.Length - 1][r] = del;
            }

            for (int thisLayerId = Layers.Length - 2; thisLayerId >= 0; thisLayerId--)
            {
                var nextLayerId = thisLayerId + 1;
                var thisLayer = Layers[thisLayerId];
                var nextLayer = Layers[nextLayerId];

                var w = DeltaWeights[thisLayerId];

                for (int h1 = 0; h1 < thisLayer.Length; h1++)
                {
                    var d = thisLayer[h1].Derivative(XW(thisLayerId, h1));
                    ErrW[thisLayerId][h1] = 0;
                    for (int h2 = 0; h2 < nextLayer.Length; h2++)
                    {
                        ErrW[thisLayerId][h1] += d * w[h2, h1] * ErrW[nextLayerId][h2];
                    }

                    for (int h2 = 0; h2 < nextLayer.Length; h2++)
                    {                        
                        w[h2, h1] += learningRate * ErrW[nextLayerId][h2] * PerceptronOutputs[thisLayerId][h1];
                    }
                }
            }
        }

        private void Process(double[] x, double[] t, bool batch)
        {
            PropagateInformation(x);            
            if (!batch)
                PropagateError(x, t);
            else
                PropagateErrorBatch(x, t);
        }

        public History Train(IEnumerable<(double[] x, double[] t)> data, int batchSize = 0, int epochsCount = 200,
            Func<double[], double[], double> lossFunction = null)
        {
            var history = new History();

            lossFunction = lossFunction ?? LossFunctions.MeanSquaredError;
            if (batchSize == 0)
            {
                for (int i = 0; i < epochsCount; i++)
                {
                    Debug.WriteLine($"[ANN] Epoch {i}");                    
                    foreach (var (x, t) in data)                    
                        Process(x, t, batch: false);                    
                    
                    double loss = 0;
                    foreach (var (x, t) in data)
                    {
                        var o = PredictSingle(x);
                        loss += lossFunction(t, o);                      
                    }
                    history.Loss.Add(loss);
                }
            }
            else
            {
                var batches = data.Select((d, i) => (data: d, batchId: i % batchSize))
                    .GroupBy(_ => _.batchId)
                    .Select(_ => _.Select(v => v.data).ToList())
                    .ToList();

                for (int i = 0; i < epochsCount; i++)
                {
                    Debug.WriteLine($"[ANN] Epoch {i}");
                    foreach (var batch in batches)
                    {
                        foreach (var (x, t) in batch)
                        {
                            Process(x, t, batch: true);
                        }
                        CommitDeltaWeights();


                        double loss = 0;
                        foreach (var (x, t) in data)
                        {
                            var o = PredictSingle(x);
                            loss += lossFunction(t, o);
                        }
                        history.Loss.Add(loss);
                    }
                }

            }
            return history;
        }

        public History Train(IEnumerable<double[]> _inputs, IEnumerable<double[]> _outputs, int batchSize = 0, int epochsCount = 200,
            Func<double[], double[], double> lossFunction = null)
        {
            var inputs = _inputs.ToArray();
            var outputs = _outputs.ToArray();
            var data = inputs.Zip(outputs, (i, o) => (x: i, t: o));
            return Train(data, batchSize:batchSize, epochsCount: epochsCount, lossFunction:lossFunction);
        }

        public double[] PredictSingle(double[] input)
        {
            PropagateInformation(input);
            return PerceptronOutputs.Last().ToArray();
        }

        private void CommitDeltaWeights()
        {
            for (int k = 0; k < Weights.Length; k++)
            {
                var w = Weights[k];
                var d = DeltaWeights[k];
                for (int i = 0; i < w.RowsCount; i++)
                {
                    for (int j = 0; j < w.ColumnsCount; j++) 
                    {
                        w[i, j] += d[i, j];
                        d[i, j] = 0;
                    }
                }
            }

        }

        public static RuntimeAnn CompileFromModel(AnnModel model)
        {
            var layers = new List<Perceptron[]>();
            var weights = new List<Matrix<double>>();            

            layers.Add(CompileLayer(new AnnLayerModel(typeof(Input), model.InputLength)));
            var prevDim = model.InputLength;

            var nextLayers = model.HiddenLayers.ToList();
            nextLayers.Add(new AnnLayerModel(typeof(Output), model.OutputLength));

            var rand = new Random();
            foreach (var layer in nextLayers) 
            {
                var m = new Matrix<double>(layer.PerceptronsCount, prevDim);
                m = Matrices.DoEachItem(m, x => rand.NextDouble());
                weights.Add(m);

                layers.Add(CompileLayer(layer));
                prevDim = layer.PerceptronsCount;
            }

            return new RuntimeAnn(layers.ToArray(), weights.ToArray());
        }

        private static Perceptron[] CompileLayer(AnnLayerModel layerModel)
            => Enumerable.Range(0, layerModel.PerceptronsCount)
                .Select(_ => Activator.CreateInstance(layerModel.PerceptronType) as Perceptron)
                .ToArray();


        public void SetInput(double[] input)
        {
            if (input.Length != InputLayer.Length)
                throw new ArgumentException("Invalid input length");

            for (int i = 0; i < input.Length; i++)
                InputLayer[i].Value = input[i];
        }

        public double[] ComputeOutput()
        {
            var values = new Matrix<double>(InputLayer.Length, 1, InputLayer.Select(_ => _.Value).ToArray());
            for (int i = 1; i < Layers.Length; i++)
            {
                values = Matrices.Multiply(Weights[i - 1], values);
            }
            return values.Items.ToArray();
        }

        public double[] ComputeOutput(double[] input)
        {
            SetInput(input);
            return ComputeOutput();
        }      
    }    
}
