using LillyScan.Backend.Types;
using System;

namespace LillyScan.Backend.AI.Layers.TfConfigConverters
{
    internal static class ActivationFunctionConverter
    {
        public static Activations.Activation Convert(string name)
        {
            var type = NameSolver.GetType(name, typeof(Activations.Activation));
            if (type == null)
                throw new TfConfigConverterFailedException($"No activation function named `{name}`");
            Console.WriteLine(type);
            try
            {
                var activation = (Activations.Activation)Activator.CreateInstance(type);
                return activation;
            }
            catch(Exception e)
            {
                throw new TfConfigConverterFailedException($"Failed creating activation function `{name}`");
            }
        }
    }
}
