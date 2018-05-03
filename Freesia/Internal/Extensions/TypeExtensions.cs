using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Freesia.Internal.Extensions
{
    internal static class TypeExtensions
    {
        private static Dictionary<Type, PropertyInfo[]> _cachedProperties = new Dictionary<Type, PropertyInfo[]>();

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
            return type.GetUnderlyingEnumerableType()?.GenericTypeArguments[0];
        }

        public static IEnumerable<PropertyInfo> GetCachedRuntimeProperties(this Type type)
        {
            if (_cachedProperties.ContainsKey(type))
                return _cachedProperties[type];
            var props = type.GetRuntimeProperties().ToArray();
            _cachedProperties[type] = props;
            return props;
        }

        public static PropertyInfo GetPreferredPropertyType(this Type targetType, string propname)
        {
            return targetType?.GetCachedRuntimeProperties().FirstOrDefault(p => p.Name.CompareIgnoreCaseTo(propname));
        }

        public static bool IsAssignableFrom(this Type from, Type to)
        {
            if (from == null || to == null) return false;
            return from.GetTypeInfo().IsAssignableFrom(to.GetTypeInfo());
        }
    }
}
