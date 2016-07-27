using System.Collections.Generic;
using System.Linq;
using Freesia.Internal.Extensions;
using Freesia.Internal.Reflection;
using Freesia.Types;

namespace Freesia.Internal
{
    internal class CodeCompletion<T> : CompilerConfig<T>
    {
        public static IEnumerable<string> Completion(string text, out string prefix)
        {
            var syntax = FilterCompiler<T>.SyntaxHighlight(text).ToArray();
            var last = syntax.LastOrDefault();
            prefix = "";
            // 末尾がnullならtypeof(T)のプロパティ
            if (last == null) return typeof(T).GetCachedRuntimeProperties()
                 .Select(p => p.Name)
                 .Concat(string.IsNullOrEmpty(UserFunctionNamespace) ? Enumerable.Empty<string>() : new[] { UserFunctionNamespace })
                 .Select(s => s.ToLowerInvariant())
                 .OrderBy(s => s);
            // 末尾がstring,(),indexerなら空
            if (last.SubType == TokenType.String) return Enumerable.Empty<string>();
            // プロパティ/メソッドを検索
            var type = last.TypeInfo;
            var lookup = last.Value?.ToLowerInvariant();
            if (last.SubType == TokenType.PropertyAccess)
            {
                lookup = "";
            }
            if (last.Type == SyntaxType.Error)
            {
                var token = syntax.Reverse().Skip(1).FirstOrDefault();
                type = token == null || IsBooleanOperator(token) ?
                    typeof(T) : token.TypeInfo;
            }
            if (type == null) return Enumerable.Empty<string>();
            prefix = lookup;
            return type.GetCachedRuntimeProperties()
                .Select(p => p.Name)
                .Concat(type == typeof(T) && !string.IsNullOrEmpty(UserFunctionNamespace) ? new[] { UserFunctionNamespace } : Enumerable.Empty<string>())
                .Concat(type == typeof(UserFunctionTypePlaceholder) ? Functions.Keys : Enumerable.Empty<string>())
                .Concat(type.IsEnumerable() ? Helper.GetEnumerableExtendedMethods() : Enumerable.Empty<string>())
                .Select(s => s.ToLowerInvariant())
                .Where(n => n.StartsWith(lookup))
                .OrderBy(s => s);
        }

        private static bool IsBooleanOperator(SyntaxInfo token)
        {
            return token.Type == SyntaxType.Operator
                && token.SubType != TokenType.PropertyAccess
                && token.SubType != TokenType.InvokeMethod;
        }
    }
}
