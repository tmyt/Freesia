using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Freesia.Internal.Extensions;
using Freesia.Internal.Types;
using Freesia.Types;

namespace Freesia.Internal
{
    internal class CodeCompletion<T> : FilterCompiler<T>
    {
        private static IEnumerable<SyntaxInfo> TakeSymbols(IEnumerable<SyntaxInfo> list)
        {
            var indexer = 0;
            foreach (var a in list)
            {
                if (a.SubType == TokenType.IndexerStart) { indexer++; yield return a; }
                else if (a.SubType == TokenType.IndexerEnd) { indexer--; yield return a; }
                else if (a.SubType == TokenType.Symbol) yield return a;
                else if (a.SubType == TokenType.PropertyAccess) yield return a;
                else if (indexer > 0) yield return a;
                else yield break;
            }
        }

        public static IEnumerable<string> CompletionInternal(string text, out string prefix)
        {
            var c = new Tokenizer(text);
            var syntax = SyntaxHighlight(c.Parse(true)).ToArray();
            var q = TakeSymbols(syntax.Reverse()).ToList();
            prefix = "";
            if (text.EndsWith("'") || text.EndsWith("\"")) return new List<string>();
            var last = q.FirstOrDefault();
            // 末尾が文字列なら空
            if (syntax.LastOrDefault()?.Type == SyntaxType.String) return new List<string>();
            // 末尾がnullならtypeof(T)のプロパティ
            if (last == null) return typeof(T).GetRuntimeProperties()
                 .Select(p => p.Name)
                 .Concat(string.IsNullOrEmpty(UserFunctionNamespace) ? Enumerable.Empty<string>() : new[] { UserFunctionNamespace })
                 .Select(s => s.ToLowerInvariant())
                 .OrderBy(s => s);
            // 末尾が '[', ']' なら空
            if (last.SubType == TokenType.IndexerStart) return new List<string>();
            if (last.SubType == TokenType.IndexerEnd) return new List<string>();
            // 最後が '.' ならプロパティを見る
            var lookup = last.SubType == TokenType.PropertyAccess;
            var type = default(Type);
            if (last.SubType == TokenType.PropertyAccess)
            {
                type = q.Skip(1).FirstOrDefault()?.TypeInfo;
            }
            // 末尾がエラーなら直前の要素
            if (last.Type == SyntaxType.Error)
            {
                type = q.Count > 2 ? q.Skip(2).FirstOrDefault()?.TypeInfo : typeof(T);
            }
            // 2個以上エラーは空
            if (q.Count(t => t.Type == SyntaxType.Error) > 1) return new List<string>();
            if (type == null) return new List<string>();
            // 絞り込み文字列
            var pp = prefix = lookup ? "" : q[0].Value;
            // プロパティ一覧を返却
            return type.GetRuntimeProperties()
                .Select(p => p.Name)
                .Concat(type == typeof(T) && !string.IsNullOrEmpty(UserFunctionNamespace) ? new[] { UserFunctionNamespace } : Enumerable.Empty<string>())
                .Concat(type == typeof(UserFunctionTypePlaceholder) ? Functions.Keys : Enumerable.Empty<string>())
                .Concat(type.IsEnumerable() ? ExtensionMethods.Methods.Keys : Enumerable.Empty<string>())
                .Select(s => s.ToLowerInvariant())
                .Where(n => n.StartsWith(pp))
                .OrderBy(s => s);
        } 
    }
}
