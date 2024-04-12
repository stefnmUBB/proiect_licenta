using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.Types;
using System.Runtime.CompilerServices;

namespace LillyScan.Backend
{
    public class Initializer
    {
        public static void Initialize()
        {
            NameSolver.Initialize();
            foreach (var method in typeof(UnsafeOperations).GetMethods())
                RuntimeHelpers.PrepareMethod(method.MethodHandle);            
        }
    }
}
