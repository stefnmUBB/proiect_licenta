using System;

namespace LillyScan.Backend.AI.Layers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple =true)]
    public class TfConfigPropertyAttribute : Attribute
    {
        public string Key { get; }
        public string Property { get; }
        public Type Converter { get; }
        public TfConfigPropertyAttribute(string key, string property = null, Type converter = null)
        {
            Key = key;
            Property = property;
            Converter = converter;
        }        
    }
}
