using System;
using System.Collections.Generic;
using System.Linq;
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
                .ToList());

        private static MethodInfo MakePreferredMethod(MethodInfo m, Type[] argTypes)
        {
            var @params = m.GetParameters();
            var generics = m.GetGenericArguments().ToDictionary(x => x.Name, x => (Type)null);
            for (var i = 0; i < @params.Length; ++i)
            {
                var t = @params[i].ParameterType;
                if (t.IsConstructedGenericType)
                {
                    var g = t.GenericTypeArguments;
                    for (var j = 0; j < g.Length; ++j)
                    {
                        if (!g[j].IsGenericParameter) continue;
                        if (generics[g[j].Name] != null)
                        {
                            if (generics[g[j].Name] != argTypes[i].GenericTypeArguments[j])
                                return null;
                            continue;
                        }
                        generics[g[j].Name] = argTypes[i].GenericTypeArguments[j];
                    }
                }
                else
                {
                    if (@params[i].ParameterType != argTypes[i]) return null;
                }
            }
            return m.MakeGenericMethod(m.GetGenericArguments().Select(x => generics[x.Name]).ToArray());
        }

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

        public static MethodInfo FindPreferredMethod(string methodName, Type[] argTypes)
        {
            var name = methodName.ToLowerInvariant();
            return EnumerableMethods.Value.Where(m => m.Name.ToLowerInvariant() == name)
                .Where(m => m.GetParameters().Length == argTypes.Length)
                .Select(m => MakePreferredMethod(m, argTypes))
                .FirstOrDefault(x => x != null);
        }
    }
}
