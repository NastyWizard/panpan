
using System.Reflection;

namespace panpan.Util
{
    public class Utility
    {
        public static IEnumerable<Type> GetAllSubTypes(Type baseType)
        {
            var allTypes = Assembly.GetExecutingAssembly().GetTypes();

            return allTypes.Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType));
        }

        public static object? CreateDefaultFromType(Type t)
        {
            if (Nullable.GetUnderlyingType(t) != null)
                return null;

            if (t.IsValueType)
                return Activator.CreateInstance(t);

            if (t == typeof(string))
                return null;

            if (t.IsArray)
                return Array.CreateInstance(t.GetElementType()!, 0);

            var ctor = t.GetConstructor(Type.EmptyTypes);
            if (ctor != null)
                return Activator.CreateInstance(t);

            return null;
        }
    }
}
