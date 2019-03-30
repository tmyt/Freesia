using System;
using System.Linq;
using System.Reflection;

namespace Freesia.Types
{
    public struct CompletionResult
    {
        private static string Mangle(string name)
        {
            switch (name)
            {
                case "Char": return "char";
                case "String": return "string";
                case "Boolean": return "bool";
                case "Single": return "float";
                case "Double": return "double";
                case "Byte": return "byte";
                case "SByte": return "sbyte";
                case "Int16": return "short";
                case "UInt16": return "ushort";
                case "Int32": return "int";
                case "UInt32": return "uint";
                case "Int64": return "long";
                case "UInt64": return "ulong";
                default: return name;
            }
        }

        private static string Mangle(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                return typeInfo.IsArray ? $"{Mangle(typeInfo.GetElementType().Name)}[]" : Mangle(type.Name);
            }
            if (typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return Mangle(type.GenericTypeArguments[0]);
            }
            var typeName = type.Name.Split('`')[0];
            var typeArguments = type.GenericTypeArguments.Select(Mangle);
            return $"{typeName}<{string.Join(", ", typeArguments)}>";
        }

        public static CompletionResult Property(Type valueType, string name)
        {
            return new CompletionResult(MemberType.Property, Mangle(valueType), name);
        }

        public static CompletionResult Method(Type valueType, string name)
        {
            return new CompletionResult(MemberType.Method, Mangle(valueType), name);
        }

        internal CompletionResult(MemberType memberType, string valueType, string name)
        {
            MemberType = memberType;
            ValueType = valueType;
            Name = name.ToLowerInvariant();
        }
        public MemberType MemberType { get; }
        public string ValueType { get; }
        public string Name { get; }
    }
}
