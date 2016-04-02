using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Freesia.Internal.Reflection
{
    internal class Helper
    {
        public static Lazy<List<MethodInfo>> EnumerableMethods =
            new Lazy<List<MethodInfo>>(() => typeof(Enumerable).GetRuntimeMethods()
                .Where(m => m.IsPublic && m.IsStatic) // only public static
                .Where(m => m.IsDefined(typeof(ExtensionAttribute), false))
                .Where(m => Nullable.GetUnderlyingType(m.ReturnType) == null)
                .Where(m =>
                {
                    var @params = m.GetParameters();
                    if (@params.Length != 2) return false;
                    var type = @params[1].ParameterType;
                    return type.IsConstructedGenericType &&
                           type.GetGenericTypeDefinition() == typeof(Func<,>);
                })
                .ToList());
        
        public static MethodInfo FindPreferredMethod(string methodName, Type argType, Type returnType)
        {
            var name = methodName.ToLowerInvariant();
            var method = EnumerableMethods.Value.Where(m => m.Name.ToLowerInvariant() == name)
                .FirstOrDefault(m =>
                {
                    var @params = m.GetParameters();
                    return @params[1].ParameterType.GenericTypeArguments[1] == returnType ||
                        @params[1].ParameterType.GenericTypeArguments[1].IsGenericParameter;
                });
            if (method == null) return null;
            switch (method.GetGenericArguments().Length)
            {
                case 1:
                    return method.MakeGenericMethod(argType);
                case 2:
                    return method.MakeGenericMethod(argType, returnType);
            }
            return null;
        }
    }
}
