using LillyScan.Backend.Math;
using LillyScan.Backend.Utils;
using System;
using System.Linq;
using System.Linq.Expressions;

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
            AssertNotNull(() => inputShapes);
            foreach (var inputShape in inputShapes)
                AssertNotNull(() => inputShape);
            Name = name;
            InputShapes = inputShapes.ToArray();
        }

        public virtual void LoadFromConfig(TfConfig config) { }        

        protected abstract Tensor<float>[] OnCall(Tensor<float>[] inputs);

        public abstract Shape[] OnGetOutputShape(Shape[] inputShapes);

        protected void Assert(Expression<Func<bool>> condition)
        {
            if (!condition.Compile()())
                throw new LayerAssertionFailedException(condition);
        }

        protected void Assert(params Expression<Func<bool>>[] conditions) => conditions.ForEach(Assert);      
        protected void AssertNotNull<T>(Expression<Func<T>> expr)
        {
            if (expr.Compile() == null)
                throw new ArgumentNullException($"Expression was null: {expr}");
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
            ValidateInputShapes(inputs.Select(_=>_.Shape).ToArray());            
            return OnCall(inputs);
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
