using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            if (last == null) return typeof(T).GetRuntimeProperties()
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
                type = syntax.Length == 1 ? typeof(T) : syntax.Reverse().Skip(1).First().TypeInfo;
            }
            if (type == null) return Enumerable.Empty<string>();
            prefix = lookup;
            return type.GetRuntimeProperties()
                .Select(p => p.Name)
                .Concat(type == typeof(T) && !string.IsNullOrEmpty(UserFunctionNamespace) ? new[] { UserFunctionNamespace } : Enumerable.Empty<string>())
                .Concat(type == typeof(UserFunctionTypePlaceholder) ? Functions.Keys : Enumerable.Empty<string>())
                .Concat(type.IsEnumerable() ? Helper.GetEnumerableExtendedMethods() : Enumerable.Empty<string>())
                .Select(s => s.ToLowerInvariant())
                .Where(n => n.StartsWith(lookup))
                .OrderBy(s => s);
        }
    }
}
