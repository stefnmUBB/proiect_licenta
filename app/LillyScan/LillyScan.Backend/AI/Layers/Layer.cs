using LillyScan.Backend.Math;
using LillyScan.Backend.Utils;
using System;
using System.Linq.Expressions;

namespace LillyScan.Backend.AI.Layers
{
    public abstract class Layer
    {
        public string Name { get; }
        public LayerRuntimeContext Context { get; } = new LayerRuntimeContext();

        protected Layer(string name)
        {
            Name = name;            
        }

        public abstract Tensor<float>[] Call(Tensor<float>[] inputs);

        public abstract Shape GetOutputShape(Shape[] inputShapes);

        protected void Assert(Expression<Func<bool>> condition)
        {
            if (!condition.Compile()())
                throw new LayerAssertionFailedException(condition);
        }

        protected void Assert(params Expression<Func<bool>>[] conditions) => conditions.ForEach(Assert);        
    }
}
