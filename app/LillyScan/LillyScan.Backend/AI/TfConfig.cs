using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace LillyScan.Backend.AI
{
    public class TfConfig
    {
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public TfConfig(Dictionary<string, object> config)
        {
            Properties = config;
        }

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


        private enum ValueType
        {
            Null=0, Int=1, String=2, Double=3, Array=4, Dictionary=5, Bool=6
        }

        private static void WriteValue(BinaryWriter bw, object value)
        {
            if(value==null)
            {
                bw.Write((byte)ValueType.Null);
                return;
            }
            if(value is int n)
            {
                bw.Write((byte)ValueType.Int);
                bw.Write(n);
                return;
            }
            if(value is double d)
            {
                bw.Write((byte)ValueType.Double);
                bw.Write(d);
                return;
            }
            if(value is string s)
            {
                bw.Write((byte)ValueType.String);
                bw.Write(s);
                return;
            }
            if(value is object[] arr)
            {
                bw.Write((byte)ValueType.Array);
                bw.Write(arr.Length);
                for (int i = 0; i < arr.Length; i++)
                    WriteValue(bw, arr[i]);
                return;
            }
            if(value is Dictionary<string,object> dict)
            {
                bw.Write((byte)ValueType.Dictionary);
                bw.Write(dict.Count);
                foreach(var p in dict)
                {
                    bw.Write(p.Key);
                    WriteValue(bw, p.Value);
                }
                return;
            }
            if(value is bool b)
            {
                bw.Write((byte)ValueType.Bool);
                bw.Write((byte)(b ? 1 : 0));
                return;
            }
            throw new NotImplementedException($"Unable to serialize object of type {value.GetType()}");
        }

        private static object ReadValue(BinaryReader br)
        {
            var typeId = (int)br.ReadByte();
            var type = Enum.IsDefined(typeof(ValueType), typeId) ? (ValueType)typeId
                : throw new InvalidOperationException($"Invalid type id {typeId}");
            switch (type)
            {
                case ValueType.Null:
                    return null; 
                case ValueType.Int:
                    return br.ReadInt32();
                case ValueType.String:
                    return br.ReadString();
                case ValueType.Double:
                    return br.ReadDouble();
                case ValueType.Array:
                    {
                        int len = br.ReadInt32();
                        var arr = new object[len];
                        for (int i = 0; i < len; i++)
                            arr[i] = ReadValue(br);
                        return arr;

                    }
                case ValueType.Dictionary:
                    {
                        int len = br.ReadInt32();
                        var d = new Dictionary<string, object>();
                        for(int i=0;i<len;i++)
                        {
                            var key = br.ReadString();
                            var value = ReadValue(br);
                            d[key] = value;
                        }
                        return d;
                    }
                case ValueType.Bool:
                    return br.ReadByte() != 0;
                default:
                    throw new NotImplementedException();
            }
        }

        internal void WriteBytes(BinaryWriter bw) => WriteValue(bw, Properties);
        internal static TfConfig ReadFromBytes(BinaryReader br) => new TfConfig(
            ReadValue(br) as Dictionary<string, object> ?? throw new InvalidOperationException("Cannot create TfConfig object"));


    }
}
