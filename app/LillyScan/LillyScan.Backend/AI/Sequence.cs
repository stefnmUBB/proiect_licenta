using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.Math;
using LillyScan.Backend.Utils;
using System;

namespace LillyScan.Backend.AI
{
    public class Sequence : Layer
    {
        private readonly Layer[] StackedLayers;
        public bool Verbose { get; set; } = true;

        public Sequence(Layer[] stackedLayers, bool verbose = true) : base(stackedLayers[0].InputShapes, null)
        {
            StackedLayers = stackedLayers;
            Verbose = verbose;
        }

        public override Shape[] OnGetOutputShape(Shape[] inputShapes)
        {
            var shapes = inputShapes;
            foreach(var l in StackedLayers)
                shapes = l.GetOutputShapes(shapes);
            return shapes;
        }        

        protected override Tensor<float>[] OnCall(Tensor<float>[] inputs)
        {
            var log = Verbose ? Console.Out : null;
            var t = inputs;
            Console.WriteLine("in: " + t.SelectShapes().JoinToString(", "));
            foreach (var l in StackedLayers)
            {
                log?.WriteLine(l);
                t = l.Call(inputs);
                Console.WriteLine("out: " + t.SelectShapes().JoinToString(", "));
            }
            return t;
        }
    }
}
