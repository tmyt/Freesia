using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Freesia.Internal.Extensions;

namespace Freesia.Internal.Reflection
{
    internal class Helper
    {
        private static Lazy<List<MethodInfo>> EnumerableMethods =
            new Lazy<List<MethodInfo>>(() => typeof(Enumerable).GetRuntimeMethods()
                .Where(m => m.IsPublic && m.IsStatic) // only public static
                .Where(m => m.IsDefined(typeof(ExtensionAttribute), false))
                .Where(m => Nullable.GetUnderlyingType(m.ReturnType) == null)
                .ToList());

        private static Lazy<List<MethodInfo>> EnumerableExtraMethods =
            new Lazy<List<MethodInfo>>(() => typeof(EnumerableEx).GetRuntimeMethods()
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
                if (t.IsConstructedGenericType && (argTypes[i]?.IsConstructedGenericType ?? false))
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
            if (!m.IsGenericMethodDefinition) return m;
            return m.MakeGenericMethod(m.GetGenericArguments().Select(x => generics[x.Name]).ToArray());
        }

        private static MethodInfo FindPreferredExtraMethod(string methodName, Type[] argTypes)
        {
            return EnumerableExtraMethods.Value.Where(m => m.Name.CompareIgnoreCaseTo(methodName))
                .Where(m => m.GetParameters().Length == argTypes.Length)
                .Select(m => MakePreferredMethod(m, argTypes))
                .FirstOrDefault(x => x != null);
        }

        private static bool MatchArgTypes(MethodInfo m, Type[] argTypes)
        {
            var paramTypes = m.GetParameters();
            if (paramTypes.Length != argTypes.Length) return false;
            return paramTypes.Select(p => p.ParameterType).SequenceEqual(argTypes);
        }

#if false // may be unused
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
#endif

        public static MethodInfo FindPreferredMethod(string methodName, Type[] argTypes)
        {
            if (methodName.CompareIgnoreCaseTo("first") || methodName.CompareIgnoreCaseTo("last"))
            {
                methodName += "ordefault";
            }
            return EnumerableMethods.Value.Where(m => m.Name.CompareIgnoreCaseTo(methodName))
                .Where(m => m.GetParameters().Length == argTypes.Length)
                .Where(m => m.IsGenericMethodDefinition || MatchArgTypes(m, argTypes))
                .Select(m => MakePreferredMethod(m, argTypes))
                .FirstOrDefault(x => x != null) ?? FindPreferredExtraMethod(methodName, argTypes);
        }

        public static IEnumerable<string> GetEnumerableExtendedMethods()
        {
            return EnumerableMethods.Value.Select(m => m.Name.ToLowerInvariant())
                .Concat(EnumerableExtraMethods.Value.Select(m => m.Name.ToLowerInvariant()))
                .Where(m => m != "firstordefault" && m != "lastordefault")
                .Distinct();
        }

        public static IEnumerable<MethodInfo> GetEnumerableExtendedMethodInfos()
        {
            return EnumerableMethods.Value.Concat(EnumerableExtraMethods.Value);
        }
    }
}
