using LillyScan.Backend.AI.Layers.TfConfigConverters;
using LillyScan.Backend.Math;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Types;
using LillyScan.Backend.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace LillyScan.Backend.AI.Layers
{
    internal class LayerInfo
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public TfConfig Config { get; set; }        
        public Shape[] WeightShapes { get; set; } = new Shape[0];
        public Tensor<float>[] Weights { get; set; }
        public LayerInfo[] Inputs { get; set; }

        public override string ToString() => $"LayerInfo(type={Type};" +
            $"name={Name};" +
            $"inputs={Inputs.Select(_ => _.Name).JoinToString(",")};" +
            $"weight_shapes={WeightShapes.JoinToString(",")};" +
            $"output={IsOutput})";

        public bool IsOutput { get; set; } = false;        

        public Layer ToLayer(Shape[] inputShapes)
        {
            var propertyFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance 
                | BindingFlags.GetProperty | BindingFlags.SetProperty;

            var layerType = NameSolver.GetType(Type, typeof(Layer));
            if (layerType == null)
                throw new InvalidOperationException($"Invalid layer type: {layerType}");

            var ctor = layerType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(c => c.GetParameters().Count() == 0).FirstOrDefault();
            if (ctor == null)
                throw new InvalidOperationException($"No parameterless constructor for type {layerType}");

            var layer = ctor.Invoke(null) as Layer;

            if (inputShapes != null)
            {
                layerType.GetProperty("InputShapes").SetValue(layer, inputShapes);
            }

            var configProperties = layerType
            .GetProperties(propertyFlags)
            .Select(p=>p.DeclaringType.GetProperty(p.Name, propertyFlags))
            .Where(p => p.GetCustomAttribute<TfConfigPropertyAttribute>() != null);

            Console.WriteLine($">> {configProperties.Select(_ => _.Name).JoinToString(", ")}");

            foreach (var property in configProperties)
            {
                var configAttr = property.GetCustomAttribute<TfConfigPropertyAttribute>();
                LoadTfConfigProperty(layer, property, configAttr);
            }

            var configClassAttributes = layerType.GetCustomAttributes<TfConfigPropertyAttribute>();
            foreach(var configAttr in configClassAttributes)
            {
                var property = layerType.GetProperty(configAttr.Property, propertyFlags)
                    .DeclaringType.GetProperty(configAttr.Property, propertyFlags);
                LoadTfConfigProperty(layer, property, configAttr);
            }

            return layer;
        }

        private void LoadTfConfigProperty(object obj, PropertyInfo prop, TfConfigPropertyAttribute attr)
        {
            var value = Config.GetValue<object>(attr.Key);

            if(attr.Converter != null)
            {
                value = ApplyConverter(attr.Converter, value, prop.PropertyType);
                prop.GetSetMethod(true).Invoke(obj, new object[] { value });
                return;                
            }
            
            if(prop.PropertyType.IsClass && value==null)
            {
                Console.WriteLine($"SetNull {prop}");
                prop.GetSetMethod(true).Invoke(obj, new object[] { null });
                return;
            }

            if (value != null && prop.PropertyType == value.GetType())
            {
                Console.WriteLine($"Set {prop} -> {value}");                
                prop.GetSetMethod(true).Invoke(obj, new object[] { value });                
            }
            else
            {
                Console.WriteLine($"Convert {prop} -> {value}");
                prop.GetSetMethod(true).Invoke(obj, new object[] { Convert.ChangeType(value, prop.PropertyType) });
            }
        }

        private object ApplyConverter(Type converter, object value, Type targetType)
        {
            value.DeepPrint();
            var flags = BindingFlags.Static | BindingFlags.Public;
            Console.WriteLine(converter.GetMethods(flags)
                .Select(m =>
                {
                    Console.WriteLine($"## {m.ReturnType} vs. {targetType} {m.ReturnType.IsAssignableFrom(targetType)} {targetType.IsAssignableFrom(m.ReturnType)}");
                    return m;
                })
                .Where(m=> m.ReturnType.IsAssignableFrom(targetType))
                .JoinToString("\n"));
            var convertMethod = (from method in converter.GetMethods(flags)
                                 where method.ReturnType.IsAssignableFrom(targetType)
                                 let pmsTypes = method.GetParameters().Select(p => p.ParameterType).ToArray()
                                 where pmsTypes.Length == 1
                                 let argType = pmsTypes[0]
                                 where (value == null && argType.IsClass)
                                    || (value != null && (value.GetType() == argType || value.GetType().IsSubclassOf(argType)))
                                 select method)
                                .FirstOrDefault();
            if (convertMethod == null)
                throw new TfConfigConverterFailedException(value, targetType);
            return convertMethod.Invoke(null, new[] { value });

        }

    }
}
