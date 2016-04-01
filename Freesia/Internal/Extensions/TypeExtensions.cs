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
            return type.GetTypeInfo().ImplementedInterfaces
                .FirstOrDefault(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        public static Type GetUnderlyingElementType(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces
                .FirstOrDefault(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof (IEnumerable<>))?.GenericTypeArguments[0];
        }
    }
}
