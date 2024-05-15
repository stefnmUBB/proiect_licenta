using LillyScan.Backend.Math;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LillyScan.Backend.AI.Layers
{    
    public abstract class Layer
    {
        [TfConfigProperty("name")]
        public string Name { get; private set; }
        public Shape[] InputShapes { get; private set; }

        public LayerRuntimeContext Context { get; } = new LayerRuntimeContext();

        internal Layer() { }

        protected Layer(Shape[] inputShapes, string name = null)
        {
            AssertNotNull("Null input shapes",() => inputShapes);
            foreach (var inputShape in inputShapes)
                AssertNotNull("Null input shape", () => inputShape);
            Name = name;
            InputShapes = inputShapes.ToArray();
        }

        public virtual void LoadFromConfig(TfConfig config) { }        

        protected abstract Tensor<float>[] OnCall(Tensor<float>[] inputs);

        public abstract Shape[] OnGetOutputShape(Shape[] inputShapes);

        protected void Assert(string message, bool condition)
        {
            if(!condition)
                throw new LayerAssertionFailedException(message);            
        }

        protected void Assert(string message, params bool[] conditions) => conditions.ForEach(_ => Assert(message, _));
        protected void AssertNotNull<T>(string message, Func<T> expr)
        {
            if (expr() == null)
                throw new ArgumentNullException($"Expression was null: {message}");
        }

        void ValidateInputShapes(Shape[] inputShapes)
        {
            if (InputShapes.Length != inputShapes.Length)
                throw new ArgumentException($"Invalid number of inputs: expected {InputShapes.Length}, got {inputShapes.Length}");
            foreach(var (placeholder, real) in InputShapes.Zip(inputShapes,(p,r)=>(p,r)))
            {
                if (!real.MatchesPlaceholder(placeholder))
                    throw new ShapeMismatchException(placeholder, real);                
            }
            OnValidateInputShapes(inputShapes);
        }

        protected virtual void OnValidateInputShapes(Shape[] inputShapes) { }        

        public Tensor<float>[] Call(params Tensor<float>[] inputs)
        {
#if DEBUG
            ValidateInputShapes(inputs.Select(_=>_.Shape).ToArray());
#endif
            var result = OnCall(inputs);
#if DEBUG
            foreach(var r in result)
            {
                for(int i=0;i< r.Buffer.Length;i++)
                {
                    if (float.IsNaN(r.Buffer[i]))
                        throw new InvalidOperationException($"Outputs NaN: {this}");
                }
            }
#endif
            /*if (X >= 0 && GetType() != typeof(LSTMCell) && GetType() != typeof(LSTM)) 
            {
                using(var f=File.CreateText($@"D:\anu3\proiect_licenta\app\LillyScan\LillyScan\bin\Debug\cc\L2\{X++}.txt"))
                //using(var f=File.CreateText($@"D:\anu3\proiect_licenta\app\LillyScan\LillyScan\bin\Debug\cc\L\{X++}_{GetType().Name}.txt"))                
                {
                    f.WriteLine(Name);
                    foreach (var r in result)                    
                        f.WriteLine($"[{r.Buffer.Buffer.JoinToString(", ")}]");                    
                }                
            }*/

            return result;
        }

        public Shape[] GetOutputShapes(params Shape[] inputShapes)
        {
            ValidateInputShapes(inputShapes);
            return OnGetOutputShape(inputShapes);
        }

        public Shape[] GetOutputShapes() => OnGetOutputShape(InputShapes);

        public override string ToString()
        {
            string ObjToString(object o)
            {
                if (o == null) return null;
                if (o.GetType().IsArray)
                    return "[" + (o as object[]).Select(ObjToString).JoinToString(",") + "]";
                return o.ToString();
            }

            var propsStr = GetType().GetProperties()
                .Where(_ => _.Name != "Context").Select(_ => $"{_.Name}={ObjToString(_.GetValue(this))}")
                .JoinToString(", ");
            return $"{GetType().Name}({propsStr})";
        }

        public virtual void OnBuild() { }        

        public void Build()
        {
            ValidateInputShapes(InputShapes);
            OnBuild();
        }

        public virtual void LoadWeights(Tensor<float>[] weights) { }
    }
}
