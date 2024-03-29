using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace LillyScan.Backend.AI.Layers
{
    [Serializable]
    internal class LayerAssertionFailedException : Exception
    {
        public Expression<Func<bool>> Condition;

        public LayerAssertionFailedException()
        {
        }

        public LayerAssertionFailedException(Expression<Func<bool>> condition) : base(condition.ToString())
        {
            Condition = condition;
        }        
    }
}