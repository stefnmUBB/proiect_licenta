using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.Math;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Utils;
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

        static int K = 0;
        static void SaveOutput(string fname, float[] b)
        {
            var path = $@"D:\anu3\proiect_licenta\app\LillyScan\LillyScan\bin\Debug\aa\{K++}_{fname}.txt";
            using(var f =File.Create(path))
            {                
                using(var r=new StreamWriter(f))
                {
                    for(int i=0;i<b.Length;i++)
                    {
                        r.Write($"{b[i]} ");
                        if (i % 100 == 99) r.WriteLine();
                    }
                }
            }
        }

        public Tensor<float>[] Call(Tensor<float>[] inputs, bool verbose = true, ProgressMonitor progressMonitor = null)
        {
            var log = verbose ? Console.Out : null;
            var solvedValues = new Dictionary<Layer, Tensor<float>[]>();
            for (int i = 0; i < inputs.Length; i++)
                solvedValues[Inputs[i]] = Inputs[i].Call(inputs[i]);
            var queue = new Queue<Layer>(InputFlow.Keys.ToArray());
            progressMonitor?.PushTask("model_call", queue.Count);
            SaveOutput("input", inputs[0].Buffer.Buffer);
            while (queue.Count > 0)
            {
                var layer = queue.Dequeue();
                log?.WriteLine($"Dequeued {layer}");
                var inputLayers = InputFlow[layer];
                if (inputLayers.Any(_ => !solvedValues.ContainsKey(_)))
                {
                    queue.Enqueue(layer);
                    log?.WriteLine($"Enqueued {layer}");
                    continue;
                }
                var crtInputs = inputLayers.SelectMany(_ => solvedValues[_]).ToArray();                

                log?.WriteLine("in: " + crtInputs.SelectShapes().JoinToString(", "));                
                var output = layer.Call(crtInputs);
                SaveOutput(layer.Name, output[0].Buffer.Buffer);
                log?.WriteLine("out: " + output.SelectShapes().JoinToString(", "));
                solvedValues[layer] = output;
                progressMonitor?.AdvanceOneStep();
            }
            progressMonitor?.PopTask();
            return Outputs.SelectMany(_ => solvedValues[_]).ToArray();
        }        
    }
}
