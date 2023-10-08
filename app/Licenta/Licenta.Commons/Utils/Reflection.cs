using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HelpersCurveDetectorDataSetGenerator.Commons.Utils
{
    public static class Reflection
    {
        public static List<Type> AppTypes;

        static Reflection()
        {
            AppTypes = (from a in AppDomain.CurrentDomain.GetAssemblies()
                        from t in a.GetTypes()
                        select t).ToList();
        }

        public static IEnumerable<Type> GetAllTypesHavingAttribute(Type attrType)
            => AppTypes.Where(t => t.GetCustomAttribute(attrType) != null);

        public static IEnumerable<MethodInfo> GetPublicStaticMethods(this Type type)
            => type.GetMethods(BindingFlags.Static | BindingFlags.Public);

        public static bool DerivesFromOrImplements(this Type type, Type baseType)
        {
            return type.IsSubclassOf(baseType) || (baseType.IsInterface && type.GetInterfaces().Contains(baseType));
        }
    }
}
