using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.Math;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.AI.Models
{
    public static class ModelLoader
    {
        static long Measure(Action a)
        {
            var sw = new Stopwatch();
            sw.Start();
            a();
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }        


        private static void LoadLayerInfosFromStream(Stream stream, out LayerInfo[] oLayers, out Dictionary<string, string[]> inputFlow,
            out LayerInfo[] outputs)
        {
            var layers = new List<LayerInfo>();
            LayerInfo currentLayer = null;
            float[] currentWeights = new float[0];
            string[] currentInputs = null;
            string[] outputLayers = null;
            inputFlow = new Dictionary<string, string[]>();
            long weightTicks = 0;
            long dictTicks = 0;

            using (TextReader r = new StreamReader(stream))
            {
                for (string line; (line = r.ReadLine()?.Trim()) != null;)
                {
                    if (line.StartsWith("[["))
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

                    if (key == "type")
                    {
                        currentLayer.Type = valueStr;
                        continue;
                    }
                    if (key == "name")
                    {
                        currentLayer.Name = valueStr;
                        continue;
                    }
                    if (key == "inputs")
                    {
                        currentInputs = valueStr.Split(';');
                        continue;
                    }
                    if (key == "weight_shapes")
                    {
                        if (valueStr == "0")
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
                    if (key == "weights")
                    {
                        weightTicks += Measure(() =>
                        currentWeights = valueStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(_ =>
                            {
                                uint num = uint.Parse(_, NumberStyles.AllowHexSpecifier);
                                byte[] floatVals = BitConverter.GetBytes(num);
                                return BitConverter.ToSingle(floatVals, 0);
                            })
                            .ToArray());
                        continue;
                    }
                    if (key == "config")
                    {
                        dictTicks += Measure(() =>
                        currentLayer.Config = new TfConfig(valueStr));
                        continue;
                    }
                    throw new InvalidOperationException($"Unknown key: {key}");
                }
            }
            Console.WriteLine("#################################");
            Console.WriteLine($"W : {weightTicks}");
            Console.WriteLine($"D : {dictTicks}");
            oLayers = layers.ToArray();

            foreach (var layer in layers)
            {
                layer.Inputs = inputFlow.GetOrDefault(layer.Name, _ => new string[0])
                    .Select(_ => layers.Where(l => l.Name == _).FirstOrDefault()
                        ?? throw new InvalidOperationException($"No layer named `{_}` has been found."))
                    .ToArray();
            }

            if (outputLayers == null)
                throw new InvalidOperationException("Outputs not specified");
            outputs = outputLayers.Select(_ => layers.Where(l => l.Name == _).FirstOrDefault()
                        ?? throw new InvalidOperationException($"No layer named `{_}` has been found."))
                    .ToArray();
            foreach (var output in outputs)
                output.IsOutput = true;
        }


        internal static void LoadLayerInfosFromBinary(BinaryReader br, out LayerInfo[] layers, out Dictionary<string, string[]> inputFlow,
            out LayerInfo[] outputs)
        {
            var layersCount = br.ReadInt32();
            layers = new LayerInfo[layersCount];
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i] = new LayerInfo();
                layers[i].Type = br.ReadString();
                layers[i].Name = br.ReadString();
                layers[i].Config = TfConfig.ReadFromBytes(br);
                layers[i].Weights = new Tensor<float>[br.ReadInt32()];
                for (int j = 0; j < layers[i].Weights.Length; j++)
                    layers[i].Weights[j] = TensorReadBytes(br);
            }            


            inputFlow = new Dictionary<string, string[]>();
            int inputFlowCount = br.ReadInt32();
            for(int i=0;i<inputFlowCount;i++)
            {
                int layerId = br.ReadInt16();
                var l1 = layers[layerId];
                int inputsLen = br.ReadInt32();
                var inputs = new string[inputsLen];
                var inLayers = new LayerInfo[inputsLen];
                for (int j = 0; j < inputsLen; j++) 
                {
                    int lid = br.ReadInt16();
                    inputs[j] = layers[lid].Name;
                    inLayers[j] = layers[lid];
                }                    
                inputFlow[l1.Name] = inputs;
                layers[layerId].Inputs = inLayers;
                
            }

            int outputsLen = br.ReadInt32();
            outputs = new LayerInfo[outputsLen];
            for(int i=0;i<outputsLen;i++)
            {
                outputs[i] = layers[br.ReadInt16()];
            }
        }

        public static byte[] StreamToBytes(Stream stream)
        {
            using(var ms=new MemoryStream())
            using(var bw = new BinaryWriter(ms))
            {
                Dictionary<LayerInfo, short> layerId = new Dictionary<LayerInfo, short>();
                Dictionary<string, short> nameId = new Dictionary<string, short>();
                LoadLayerInfosFromStream(stream, out var layers, out var inputFlow, out var _outputs);
                bw.Write(layers.Length);                
                for(short i =0;i<layers.Length;i++)                
                {
                    var layer = layers[i];
                    bw.Write(layer.Type);
                    bw.Write(layer.Name);
                    layer.Config.WriteBytes(bw);
                    bw.Write(layer.Weights.Length);
                    foreach (var w in layer.Weights)
                        TensorWriteBytes(bw, w);
                    layerId[layer] = i;
                    nameId[layer.Name] = i;
                }

                bw.Write(inputFlow.Count);
                foreach(var kv in inputFlow)
                {
                    bw.Write(nameId[kv.Key]);
                    bw.Write(kv.Value.Length);
                    foreach (var v in kv.Value)
                        bw.Write(nameId[v]);
                }                

                bw.Write(_outputs.Length);
                foreach(var o in _outputs)
                {
                    bw.Write(layerId[o]);
                }

                return ms.ToArray();
            }
        }

        private static void TensorWriteBytes(BinaryWriter bw, Tensor<float> tensor)
        {
            bw.Write(tensor.Shape.Length);
            for (int i = 0; i < tensor.Shape.Length; i++)
                bw.Write(tensor.Shape[i]);
            bw.Write(tensor.Buffer.Length);
            for (int i = 0; i < tensor.Buffer.Length; i++)
                bw.Write(tensor.Buffer[i]);
        }

        private static Tensor<float> TensorReadBytes(BinaryReader br)
        {
            var shape = new int[br.ReadInt32()];            
            for(int i=0;i<shape.Length;i++)
                shape[i] = br.ReadInt32();
            var elemsCount = br.ReadInt32();
            var elems = new float[elemsCount];
            for (int i = 0; i < elemsCount; i++)
                elems[i] = br.ReadSingle();
            return new Tensor<float>(shape, elems);
        }


        private static void BuildLayers(LayerInfo[] layers, Dictionary<string, string[]> inputFlow, LayerInfo[] _outputs,
            out Layer[] inputs, out Layer[] outputs, out Dictionary<Layer, Layer[]> layerInputs)
        {
            var solvedLayers = new Dictionary<LayerInfo, Layer>();
            var queue = new Queue<LayerInfo>(layers.ToArray());
            var _inputs = layers.Where(li => li.Type == "InputLayer")
                .Select(li => solvedLayers[li] = li.ToLayer(null))
                .ToArray();
            Dictionary<Layer, Layer[]> _layerInputs = new Dictionary<Layer, Layer[]>();
            while (queue.Count > 0)
            {
                var li = queue.Dequeue();
                Console.WriteLine($"Dequeued {li.Name}");
                if (solvedLayers.ContainsKey(li))
                    continue;
                if (li.Inputs.Any(_ => !solvedLayers.ContainsKey(_)))
                {
                    Console.WriteLine($"Enqueued {li.Name}");
                    queue.Enqueue(li);
                    continue;
                }
                var lInputs = li.Inputs.Select(_ => solvedLayers[_]).ToArray();
                var inputShapes = lInputs.SelectMany(_ => _.GetOutputShapes()).ToArray();
                inputShapes.DeepPrint();
                var layer = li.ToLayer(inputShapes);
                _layerInputs[layer] = lInputs;
                solvedLayers[li] = layer;
                //Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Layer {layer} outputs {layer.GetOutputShapes().JoinToString(", ")}");
                //Console.ForegroundColor = ConsoleColor.White;
            }

            inputs = _inputs;
            outputs = _outputs.Select(_ => solvedLayers[_]).ToArray();
            layerInputs = _layerInputs;
        }

        private static void LoadFromStream(Stream stream, out Layer[] inputs, out Layer[] outputs, out Dictionary<Layer, Layer[]> layerInputs)
        {
            LoadLayerInfosFromStream(stream, out var layers, out var inputFlow, out var _outputs);
            BuildLayers(layers, inputFlow, _outputs, out inputs, out outputs, out layerInputs);            
        }

        private static void LoadFromBinary(BinaryReader br, out Layer[] inputs, out Layer[] outputs, out Dictionary<Layer, Layer[]> layerInputs)
        {
            LoadLayerInfosFromBinary(br, out var layers, out var inputFlow, out var _outputs);            
            BuildLayers(layers, inputFlow, _outputs, out inputs, out outputs, out layerInputs);
        }

        public static Model LoadFromStream(Stream stream)
        {
            LoadFromStream(stream, out var inputs, out var outputs, out var layerInputs);
            return new Model(inputs, outputs, layerInputs);
        }

        public static Model LoadFromString(string plainText)
        {
            using(var ms=new MemoryStream(Encoding.UTF8.GetBytes(plainText)))
            {
                return LoadFromStream(ms);
            }
        }  

        public static Model LoadFromBinary(BinaryReader br)
        {
            LoadFromBinary(br, out var inputs, out var outputs, out var layerInputs);
            return new Model(inputs, outputs, layerInputs);
        }


        public static Model LoadFromBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            using (var br = new BinaryReader(ms))
                return LoadFromBinary(br);
        }
    }
}
