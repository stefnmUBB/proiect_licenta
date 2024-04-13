using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.Math;
using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LillyScan.Backend.AI.Models
{
    public class Model
    {
        public readonly Layer[] Inputs;
        public readonly Layer[] Outputs;

        private readonly Dictionary<Layer, Layer[]> InputFlow;

        public Model(Layer[] inputs, Layer[] outputs, Dictionary<Layer, Layer[]> inputFlow)
        {
            Inputs = inputs;
            Outputs = outputs;
            InputFlow = inputFlow;
        }

        public Tensor<float>[] Call(params Tensor<float>[] inputs)
        {
            var solvedValues = new Dictionary<Layer, Tensor<float>[]>();
            for (int i = 0; i < inputs.Length; i++)
                solvedValues[Inputs[i]] = Inputs[i].Call(inputs[i]);
            var queue = new Queue<Layer>(InputFlow.Keys.ToArray());
            while (queue.Count > 0)
            {
                var layer = queue.Dequeue();
                Console.WriteLine($"Dequeued {layer}");
                var inputLayers = InputFlow[layer];
                if (inputLayers.Any(_ => !solvedValues.ContainsKey(_)))
                {
                    queue.Enqueue(layer);
                    Console.WriteLine($"Enqueued {layer}");
                    continue;
                }
                var crtInputs = inputLayers.SelectMany(_ => solvedValues[_]).ToArray();

                Console.WriteLine("in: " + crtInputs.SelectShapes().JoinToString(", "));                
                var output = layer.Call(crtInputs);
                Console.WriteLine("out: " + output.SelectShapes().JoinToString(", "));
                solvedValues[layer] = output;
            }
            return Outputs.SelectMany(_ => solvedValues[_]).ToArray();
        }
        public Tensor<float>[] Call(Tensor<float>[] inputs, CancellationToken cancellationToken)
        {
            var solvedValues = new Dictionary<Layer, Tensor<float>[]>();
            for (int i = 0; i < inputs.Length; i++)
                solvedValues[Inputs[i]] = Inputs[i].Call(inputs[i]);
            var queue = new Queue<Layer>(InputFlow.Keys.ToArray());
            while (queue.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Model.Call was canceled");
                    return null;
                }
                var layer = queue.Dequeue();
                Console.WriteLine($"Dequeued {layer}");
                var inputLayers = InputFlow[layer];
                if (inputLayers.Any(_ => !solvedValues.ContainsKey(_)))
                {
                    queue.Enqueue(layer);
                    Console.WriteLine($"Enqueued {layer}");
                    continue;
                }
                var crtInputs = inputLayers.SelectMany(_ => solvedValues[_]).ToArray();

                Console.WriteLine("in: " + crtInputs.SelectShapes().JoinToString(", "));
                var output = layer.Call(crtInputs);
                Console.WriteLine("out: " + output.SelectShapes().JoinToString(", "));
                solvedValues[layer] = output;
            }
            if (cancellationToken.IsCancellationRequested)
            { 
                Console.WriteLine("Model.Call was canceled");
                return null;
            }
            return Outputs.SelectMany(_ => solvedValues[_]).ToArray();
        }

    }
}
