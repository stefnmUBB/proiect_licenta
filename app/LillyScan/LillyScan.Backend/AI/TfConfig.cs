using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;

namespace LillyScan.Backend.AI
{
    public class TfConfig
    {
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public TfConfig(string config)
        {
            Properties = PythonDictionaryParser.Parse(config);
        }        

        public T GetValue<T>(string key)
        {
            var value = Properties[key];
            if (!typeof(T).IsValueType && value == null)
                return (T)value;
            if (!(value is T))
                throw new InvalidCastException($"Expected {typeof(T)}, got {value?.GetType()?.ToString() ?? "null"}");
            return (T)value;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            var _value = Properties[key];
            if (!typeof(T).IsValueType && _value == null)
            {
                value = (T)_value;
                return true;
            }
            if (!(_value is T))
            {
                value = default;
                return false;
            }
            value = (T)_value;
            return true;
        }

        public T GetValueOrDefault<T>(string key, T defaultValue) => TryGetValue(key, out T value) ? value : defaultValue;


    }
}
