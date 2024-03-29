using System;
using System.Runtime.Serialization;

namespace LillyScan.Backend.AI.Layers.TfConfigConverters
{
    [Serializable]
    internal class TfConfigConverterFailedException : Exception
    {        

        public TfConfigConverterFailedException()
        {
        }

        public TfConfigConverterFailedException(string message) : base(message)
        {
        }

        public TfConfigConverterFailedException(object input, Type type) 
            : base($"Failed to convert to {type}: {input}")
        {            
        }        
    }
}