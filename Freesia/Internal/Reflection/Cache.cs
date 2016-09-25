using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Freesia.Internal.Reflection
{
    internal class Cache
    {
        public static Lazy<MethodInfo> RegexIsMatch { get; }
            = new Lazy<MethodInfo>(() => typeof(Regex).GetRuntimeMethod("IsMatch", new[] { typeof(string) }));
        public static Lazy<ConstructorInfo> RegexCtor { get; }
            = new Lazy<ConstructorInfo>(() => typeof(Regex).GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 2)); 
        public static Lazy<MethodInfo> StringContains { get; }
            = new Lazy<MethodInfo>(() => typeof(string).GetRuntimeMethod("Contains", new[] { typeof(string) }));
        public static Lazy<MethodInfo> StringToLowerInvariant { get; }
            = new Lazy<MethodInfo>(() => typeof(string).GetRuntimeMethod("ToLowerInvariant", new Type[0]));
        public static Lazy<MethodInfo> CharToString { get; }
         = new Lazy<MethodInfo>(() => typeof(char).GetRuntimeMethod("ToString", new Type[0]));

        // Enumerable
        public static Lazy<MethodInfo> EnumerableAny { get; }
            = new Lazy<MethodInfo>(() => typeof(Helper).GetRuntimeMethod("GetEnumerableAnyMethod", new Type[0]));
    }
}
