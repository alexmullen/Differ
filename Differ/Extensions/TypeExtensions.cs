using System;

namespace Differ.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsConsideredPrimitive(this Type type)
        {
            return type.IsPrimitive || type.IsEnum || type.IsAssignableFrom(typeof(string)) || type.IsAssignableFrom(typeof(DateTime));
        }

        public static bool IsConsideredNonPrimitive(this Type type)
        {
            return !type.IsConsideredPrimitive();
        }
    }
}
