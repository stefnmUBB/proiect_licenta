using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.Math;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.AI.Models
{
    public static class ModelLoader
    {       
        public static void LoadFromStream(Stream stream)
        {
            var layers = new List<LayerInfo>();
            LayerInfo currentLayer = null;
            float[] currentWeights = new float[0];
            string[] currentInputs = null;
            string[] outputLayers = null;
            var inputFlow = new Dictionary<string, string[]>();

            using(TextReader r =new StreamReader(stream))
            {                
                for(string line; (line = r.ReadLine()?.Trim())!=null;)
                {                    
                    if(line.StartsWith("[["))
                    {
                        if (currentLayer != null)
                        {
                            if (currentLayer.Name == null)
                                throw new InvalidOperationException("Current layer name not specified");
                            if (currentLayer.Type == null)
                                throw new InvalidOperationException("Current layer type not specified");
                            if (currentLayer.Config == null)
                                throw new InvalidOperationException("Current layer config not specified");

                            if (currentInputs != null)
                                inputFlow[currentLayer.Name] = currentInputs;
                            currentInputs = null;

                            if (currentLayer.WeightShapes.Select(_ => _.ElementsCount).DefaultIfEmpty(0).Sum() != currentWeights.Length)
                                throw new InvalidOperationException("Current layer weights count don't match shapes");

                            currentLayer.Weights = new Tensor<float>[currentLayer.WeightShapes.Length];
                            for (int i = 0, k = 0; i < currentLayer.Weights.Length; i++) 
                            {
                                var buffer = new float[currentLayer.WeightShapes[i].ElementsCount];
                                Array.Copy(currentWeights, k, buffer, 0, buffer.Length);
                                currentLayer.Weights[i] = new Tensor<float>(currentLayer.WeightShapes[i], buffer);
                                k += buffer.Length;
                            }
                            layers.Add(currentLayer);
                        }

                        if (line == "[[Layer]]")
                        {
                            currentLayer = new LayerInfo();
                            currentWeights = new float[0];
                            currentInputs = null;
                            continue;
                        }
                        if (line == "[[Outputs]]") 
                        {
                            line = r.ReadLine().Trim();
                            outputLayers = line.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                            continue;
                        }
                    }
                    if (!(line.StartsWith("[") && line.EndsWith("]")))
                        throw new InvalidOperationException($"Unable to parse: {line.Substring(0, System.Math.Min(line.Length, 100))}");
                    var key = line.Substring(1, line.Length - 2);
                    var valueStr = r.ReadLine().Trim();

                    if(key=="type")
                    {
                        currentLayer.Type = valueStr;
                        continue;
                    }
                    if(key=="name")
                    {
                        currentLayer.Name = valueStr;
                        continue;
                    }
                    if(key=="inputs")
                    {
                        currentInputs = valueStr.Split(';');
                        continue;
                    }
                    if(key=="weight_shapes")
                    {
                        if(valueStr=="0")
                        {
                            currentLayer.WeightShapes = new Shape[0];
                        }
                        else
                        {
                            currentLayer.WeightShapes = valueStr.Split(';')
                                .Select(_ => new Shape(_.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse)))
                                .ToArray();
                        }
                        continue;
                    }
                    if(key=="weights")
                    {
                        currentWeights = valueStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(_ => 
                            {
                                uint num = uint.Parse(_, NumberStyles.AllowHexSpecifier);
                                byte[] floatVals = BitConverter.GetBytes(num);
                                return BitConverter.ToSingle(floatVals, 0);
                            })
                            .ToArray();                        
                        continue;
                    }
                    if(key=="config")
                    {
                        currentLayer.Config = new TfConfig(valueStr);
                        continue;
                    }
                    throw new InvalidOperationException($"Unknown key: {key}");
                }
            }

            foreach (var layer in layers)
            {
                layer.Inputs = inputFlow.GetOrDefault(layer.Name, _ => new string[0])
                    .Select(_ => layers.Where(l => l.Name == _).FirstOrDefault()
                        ?? throw new InvalidOperationException($"No layer named `{_}` has been found."))
                    .ToArray();                
            }

            if (outputLayers == null)
                throw new InvalidOperationException("Outputs note specified");
            var outputs = outputLayers.Select(_ => layers.Where(l => l.Name == _).FirstOrDefault()
                        ?? throw new InvalidOperationException($"No layer named `{_}` has been found."))
                    .ToArray();
            foreach (var output in outputs)
                output.IsOutput = true;


            var solvedLayers = new Dictionary<LayerInfo, Layer>();            
            var queue = new Queue<LayerInfo>(layers.ToArray());
            var inputs = layers.Where(li => li.Type == "InputLayer")
                .Select(li => solvedLayers[li] = li.ToLayer(null))
                .ToArray();
            Dictionary<Layer, Layer[]> layerInputs=new Dictionary<Layer, Layer[]>();
            while (queue.Count>0)
            {
                var li = queue.Dequeue();
                Console.WriteLine($"Dequeued {li.Name}");
                if (solvedLayers.ContainsKey(li))
                    continue;
                if(li.Inputs.Any(_=>!solvedLayers.ContainsKey(_)))
                {
                    Console.WriteLine($"Enqueued {li.Name}");
                    queue.Enqueue(li);
                    continue;
                }
                var lInputs = li.Inputs.Select(_ => solvedLayers[_]).ToArray();
                var inputShapes = lInputs.SelectMany(_ => _.GetOutputShapes()).ToArray();
                inputShapes.DeepPrint();
                var layer = li.ToLayer(inputShapes);
                layerInputs[layer] = lInputs;
                solvedLayers[li] = layer;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Layer {layer} outputs {layer.GetOutputShapes().JoinToString(", ")}");
                Console.ForegroundColor = ConsoleColor.White;
            }



            /*foreach (var layer in layers)
            {
                Console.WriteLine(layer);
                try
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(layer.ToLayer(null));
                    Console.ForegroundColor = ConsoleColor.White;
                }
                catch(Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            } */


        }

    }
}
