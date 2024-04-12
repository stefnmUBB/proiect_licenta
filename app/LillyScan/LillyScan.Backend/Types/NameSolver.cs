using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LillyScan.Backend.Types
{
    public static class NameSolver
    {        
        private static Assembly PeekAssembly(Assembly a)
        {
            Console.WriteLine("Found assembly:" + a.FullName);
            return a;
        }

        private static Type[] TryLoadTypesOrNone(this Assembly a)
        {
            try
            {
                return a.GetTypes();
            }
            catch(System.Reflection.ReflectionTypeLoadException e)
            {
                Console.WriteLine($"[NameSolver] Failed to load types from {a.FullName}: {e.Message}");
                return Type.EmptyTypes;
            }
        }

        private static readonly Dictionary<string, Type[]> NamedTypes =
            (from assembly in AppDomain.CurrentDomain.GetAssemblies()
             from type in PeekAssembly(assembly).TryLoadTypesOrNone()
             let namedAttribute = type.GetCustomAttribute<NamedAttribute>()
             where namedAttribute != null
             group type by namedAttribute.Name into kv
             select kv
            )
            .ToDictionary(kv => kv.Key, kv => kv.ToArray());

        public static Type GetType(string name, Type baseType)
            => NamedTypes.TryGetValue(name, out var types) ? types.Where(t => t.IsSubclassOf(baseType)).FirstOrDefault() : default;

        public static void DumpNamedTypes()
        {
            foreach(var kv in NamedTypes)            
                Console.WriteLine($"{kv.Key} -> {kv.Value.JoinToString(", ")}");            
        }

        public static void Initialize() { }

    }
}
