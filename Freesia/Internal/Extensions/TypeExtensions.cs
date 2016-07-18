using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Freesia.Internal.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsEnumerable(this Type type)
        {
            return type.GetUnderlyingEnumerableType() != null;
        }

        public static Type GetUnderlyingEnumerableType(this Type type)
        {
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type;
            return type.GetTypeInfo().ImplementedInterfaces
                .FirstOrDefault(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        public static Type GetUnderlyingElementType(this Type type)
        {
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GenericTypeArguments[0];
            return type.GetTypeInfo().ImplementedInterfaces
                .FirstOrDefault(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))?.GenericTypeArguments[0];
        }

        public static PropertyInfo GetPreferredPropertyType(this Type targetType, string propname)
        {
            return targetType?.GetRuntimeProperties().FirstOrDefault(p => string.Compare(p.Name, propname, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
