
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
    }
}
