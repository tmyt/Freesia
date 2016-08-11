using System;
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
            var syntax = FilterCompiler.SyntaxHighlight<T>(text).ToArray();
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
            Type type = last.TypeInfo, baseType = type;
            var lookup = last.Value?.ToLowerInvariant();
            if (last.SubType == TokenType.InvokeMethod)
            {
                // メソッド呼び出しなら空
                return Enumerable.Empty<string>();
            }
            if (last.Type == SyntaxType.Operator && last.SubType != TokenType.PropertyAccess)
            {
                // 終端が演算子ならprefixと検索する型をクリア
                lookup = "";
                type = typeof(T);
            }
            if (last.SubType == TokenType.PropertyAccess)
            {
                // プロパティアクセスの手前がErrorの場合は空
                var next = syntax.Reverse().Skip(1).FirstOrDefault();
                if (next?.Type == SyntaxType.Error || (next?.Type == SyntaxType.Operator && next.SubType != TokenType.InvokeMethod))
                    return Enumerable.Empty<string>();
                // prefixをクリア
                lookup = "";
            }
            // Errorノードだった場合は直前のノードの型を検索対象とする
            if (last.Type == SyntaxType.Error || last.Type == SyntaxType.Identifier)
            {
                var token = syntax.Reverse().Skip(1).FirstOrDefault();
                baseType = token == null || IsSymbol(token) || IsBooleanOperator(token) ?
                    null : token.TypeInfo;
                type = baseType ?? typeof(T);
            }
            if (type == null) return Enumerable.Empty<string>();
            prefix = lookup;
            return type.GetCachedRuntimeProperties()
                .Select(p => p.Name)
                .Concat(baseType == null && !string.IsNullOrEmpty(UserFunctionNamespace) ? new[] { UserFunctionNamespace } : Enumerable.Empty<string>())
                .Concat(type == typeof(UserFunctionTypePlaceholder) ? Functions.Keys : Enumerable.Empty<string>())
                .Concat(type.IsEnumerable() ? Helper.GetEnumerableExtendedMethods() : Enumerable.Empty<string>())
                .Select(s => s.ToLowerInvariant())
                .Where(n => n.StartsWith(lookup))
                .OrderBy(s => s);
        }

        private static bool IsSymbol(SyntaxInfo token)
        {
            return token.SubType == TokenType.Symbol
                   || token.SubType == TokenType.String
                   || token.SubType == TokenType.Double
                   || token.SubType == TokenType.Long
                   || token.SubType == TokenType.ULong
                   || token.SubType == TokenType.Bool
                   || token.SubType == TokenType.Null;
        }

        private static bool IsBooleanOperator(SyntaxInfo token)
        {
            return token.Type == SyntaxType.Operator
                   && token.SubType != TokenType.PropertyAccess
                   && token.SubType != TokenType.InvokeMethod
                   && token.SubType != TokenType.UnaryPlus
                   && token.SubType != TokenType.UnaryMinus
                   && token.SubType != TokenType.Plus
                   && token.SubType != TokenType.Minus
                   && token.SubType != TokenType.Multiply
                   && token.SubType != TokenType.Divide
                   && token.SubType != TokenType.Modulo;
        }
    }
}
