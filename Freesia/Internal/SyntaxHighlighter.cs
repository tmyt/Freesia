using System;
using System.Collections.Generic;
using System.Linq;
using Freesia.Internal.Extensions;
using Freesia.Internal.Reflection;
using Freesia.Internal.Types;
using Freesia.Types;

namespace Freesia.Internal
{
    internal class SyntaxHighlighter<T> : CompilerConfig<T>
    {
        private static SyntaxInfo TranslateSyntaxInfo(CompilerToken t)
        {
            switch (t.Type)
            {
                case TokenType.Equals:
                case TokenType.EqualsI:
                case TokenType.NotEquals:
                case TokenType.NotEqualsI:
                case TokenType.Regexp:
                case TokenType.Contains:
                case TokenType.ContainsI:
                case TokenType.And:
                case TokenType.Or:
                case TokenType.Not:
                case TokenType.LessThan:
                case TokenType.GreaterThan:
                case TokenType.LessThanEquals:
                case TokenType.GreaterThanEquals:
                case TokenType.OpenBracket:
                case TokenType.CloseBracket:
                case TokenType.ArrayStart:
                case TokenType.ArrayEnd:
                case TokenType.ArrayDelimiter:
                case TokenType.IndexerStart:
                case TokenType.IndexerEnd:
                case TokenType.Lambda:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Operator, Value = t.Value };
                case TokenType.String:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.String, Value = t.Value };
                case TokenType.Double:
                case TokenType.Long:
                case TokenType.ULong:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Constant, Value = t.Value };
                case TokenType.Bool:
                case TokenType.Null:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Keyword, Value = t.Value };
                default:
                    throw new ParseException("#-1", t.Position);
            }
        }

        private static IEnumerable<SyntaxInfo> ParseSymbolType(Queue<CompilerToken> symbols, Type argType, string argName)
        {
            var targetType = typeof(T);
            var indexer = 0;
            // yield break for no symbols
            if (symbols.Count == 0) yield break;
            // lambda argument
            if (symbols.First().Value == argName)
            {
                targetType = argType;
                yield return new SyntaxInfo(symbols.Dequeue(), SyntaxType.Argument) { TypeInfo = targetType };
            }
            foreach (var prop in symbols)
            {
                var propname = prop.Value;
                var syntaxType = default(SyntaxType);
                if (prop.Type == TokenType.PropertyAccess)
                {
                    yield return new SyntaxInfo(prop, SyntaxType.Operator) { Value = "." };
                    continue;
                }
                if (prop.Type == TokenType.IndexerStart)
                {
                    indexer++;
                    yield return TranslateSyntaxInfo(prop);
                    continue;
                }
                if (prop.Type == TokenType.IndexerEnd)
                {
                    indexer--;
                    var s = TranslateSyntaxInfo(prop);
                    if (indexer == 0)
                    {
                        s.TypeInfo = targetType.GetUnderlyingElementType();
                        targetType = s.TypeInfo;
                    }
                    yield return s;
                    continue;
                }
                if (indexer > 0)
                {
                    yield return TranslateSyntaxInfo(prop);
                    continue;
                }
                if (propname.ToLowerInvariant() == UserFunctionNamespace && targetType == typeof(T))
                {
                    syntaxType = SyntaxType.Identifier;
                    targetType = typeof(UserFunctionTypePlaceholder);
                }
                else if (targetType == typeof(UserFunctionTypePlaceholder))
                {
                    syntaxType = Functions.ContainsKey(prop.Value) ? SyntaxType.Identifier : SyntaxType.Error;
                    targetType = null;
                }
                else
                {
                    var propInfo = targetType.GetPreferredPropertyType(propname);
                    syntaxType = propInfo == null ? SyntaxType.Error : SyntaxType.Identifier;
                    if (syntaxType == SyntaxType.Error && (targetType?.IsEnumerable() ?? false))
                    {
                        syntaxType = Helper.EnumerableMethods.Value.Any(m => m.Name.ToLowerInvariant() == prop.Value.ToLowerInvariant())
                            ? SyntaxType.Identifier
                            : SyntaxType.Error;
                        targetType = null;
                    }
                    else
                    {
                        targetType = propInfo?.PropertyType;
                    }
                }
                yield return new SyntaxInfo(prop, syntaxType) { TypeInfo = targetType };
                if (syntaxType == SyntaxType.Error) yield break;
            }
        }

        private static IEnumerable<CompilerToken> TakeSymbols(IEnumerator<CompilerToken> list)
        {
            var indexer = 0;
            while (list.MoveNext())
            {
                var a = list.Current;
                if (a.Type == TokenType.IndexerStart) { indexer++; yield return a; }
                else if (a.Type == TokenType.IndexerEnd) { indexer--; yield return a; }
                else if (a.Type == TokenType.Symbol) yield return a;
                else if (a.Type == TokenType.PropertyAccess) yield return a;
                else if (indexer > 0) yield return a;
                else yield break;
            }
        }

        public static IEnumerable<SyntaxInfo> SyntaxHighlight(IEnumerable<CompilerToken> tokenList)
        {
            var pendingSymbols = new Queue<CompilerToken>();
            var lambdaParsing = false;
            var brackets = 0;
            var argstack = new Stack<Tuple<string, Type, int>>();
            var argname = default(string);
            var argtype = default(Type);
            var latestResolvedType = default(Type);
            var enumerator = tokenList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var t = enumerator.Current;
                // read symbols
                if (t.Type == TokenType.Symbol || t.Type == TokenType.PropertyAccess)
                {
                    pendingSymbols.Enqueue(t);
                    foreach (var s in TakeSymbols(enumerator)) pendingSymbols.Enqueue(s);
                    try
                    {
                        t = enumerator.Current;
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }
                }
                // enter Lambda parsing mode
                if (t.Type == TokenType.Lambda)
                {
                    // save current state
                    if (lambdaParsing)
                    {
                        argstack.Push(Tuple.Create(argname, argtype, brackets));
                    }
                    lambdaParsing = true;
                    brackets = 1;
                    var arg = pendingSymbols.Dequeue();
                    yield return new SyntaxInfo(arg, SyntaxType.Argument);
                    yield return new SyntaxInfo(t, SyntaxType.Operator);
                    argname = arg.Value;
                    argtype = latestResolvedType?.GetUnderlyingElementType();
                    pendingSymbols.Clear();
                    continue;
                }
                if (lambdaParsing)
                {
                    if (t.Type == TokenType.OpenBracket) brackets++;
                    if (t.Type == TokenType.CloseBracket) brackets--;
                }
                if (pendingSymbols.Count > 0)
                {
                    foreach (var a in ParseSymbolType(pendingSymbols, argtype, argname))
                    {
                        if (a.TypeInfo != null) latestResolvedType = a.TypeInfo;
                        yield return a;
                    }
                    pendingSymbols.Clear();
                }
                if (lambdaParsing && brackets == 0)
                {
                    lambdaParsing = false;
                    argname = null;
                    // restore previews environment
                    if (argstack.Count > 0)
                    {
                        var prev = argstack.Pop();
                        argname = prev.Item1;
                        argtype = prev.Item2;
                        brackets = prev.Item3;
                        lambdaParsing = true;
                    }
                }
                // process rest of token
                if (t.Type != TokenType.Symbol && t.Type != TokenType.PropertyAccess)
                {
                    yield return TranslateSyntaxInfo(t);
                }
            }
            // 残ってたら全部出す
            foreach (var a in ParseSymbolType(pendingSymbols, argtype, argname)) yield return a;
        }
    }
}
