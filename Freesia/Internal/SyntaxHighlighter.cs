using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Freesia.Internal.Extensions;
using Freesia.Internal.Reflection;
using Freesia.Internal.Types;
using Freesia.Types;

namespace Freesia.Internal
{
    internal class SyntaxHighlighter<T> : CompilerConfig<T>
    {
        private class ExtendedMethodPlaceholder { }

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
                case TokenType.NotRegexp:
                case TokenType.NotContains:
                case TokenType.NotContainsI:
                case TokenType.OpenBracket:
                case TokenType.CloseBracket:
                case TokenType.ArrayStart:
                case TokenType.ArrayEnd:
                case TokenType.ArrayDelimiter:
                case TokenType.IndexerStart:
                case TokenType.IndexerEnd:
                case TokenType.Lambda:
                case TokenType.ArrayNode:
                case TokenType.IndexerNode:
                case TokenType.PropertyAccess:
                case TokenType.InvokeMethod:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Operator, Value = t.Value, TypeInfo = null };
                case TokenType.String:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.String, Value = t.Value, TypeInfo = typeof(string) };
                case TokenType.Double:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Constant, Value = t.Value, TypeInfo = typeof(double) };
                case TokenType.Long:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Constant, Value = t.Value, TypeInfo = typeof(long) };
                case TokenType.ULong:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Constant, Value = t.Value, TypeInfo = typeof(ulong) };
                case TokenType.Bool:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Keyword, Value = t.Value, TypeInfo = typeof(bool) };
                case TokenType.Null:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Keyword, Value = t.Value, TypeInfo = null };
                case TokenType.Symbol:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Identifier, Value = t.Value, TypeInfo = null /* to be determin */ };
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
                        syntaxType = Helper.GetEnumerableExtendedMethods().Any(m => m == prop.Value.ToLowerInvariant())
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

        private static Type GetPropertyType(Type type, string name)
        {
            return type.GetRuntimeProperties().Where(p => p.Name.ToLowerInvariant() == name.ToLowerInvariant()).Select(p => p.PropertyType).FirstOrDefault();
        }

        private static bool IsExtendedMethod(Type type, string name)
        {
            if (!type.IsEnumerable()) return false;
            return Helper.GetEnumerableExtendedMethods().Any(m => name == m);
        }

        private static IEnumerable<MethodInfo> GetMethodInfo(Type type, string name)
        {
            return Helper.GetEnumerableExtendedMethodInfos().Where(m => m.Name.ToLowerInvariant() == name);
        }

        private static IEnumerable<SyntaxInfo> HighlightOne(ASTNode node, Type parentNodeType = null, ASTNode lambdaArg = null)
        {
            if (node == null) yield break;
            if (node.Token.Type == TokenType.Nop) yield break;
            var info = TranslateSyntaxInfo(node.Token);
            parentNodeType = parentNodeType ?? typeof(T);

            // determine identifier type
            if (info.Type == SyntaxType.Identifier && info.TypeInfo == null)
            {
                info.TypeInfo = GetPropertyType(parentNodeType, info.Value);
                if (lambdaArg != null && node.Token.Value == lambdaArg.Token.Value)
                {
                    info.TypeInfo = node.DeterminedType = lambdaArg.DeterminedType;
                    info.Type = SyntaxType.Identifier;
                }
                if (info.TypeInfo == null)
                {
                    info.TypeInfo = IsExtendedMethod(parentNodeType, info.Value) ? typeof(ExtendedMethodPlaceholder) : null;
                }
                if (info.TypeInfo == null)
                {
                    info.Type = SyntaxType.Error;
                }
            }
            // override node type info
            if (node.DeterminedType == null) node.DeterminedType = info.TypeInfo;

            // process left node (and update ``node.Left.DeterminedType'' value)
            var lhs = HighlightOne(node.Left, parentNodeType, lambdaArg).ToList();
            // process right node
            var rhs = HighlightOne(node.Right, node.Left?.DeterminedType, lambdaArg).ToList();

            // determine operator node type
            if (info.Type == SyntaxType.Operator)
            {
                if (info.SubType == TokenType.PropertyAccess)
                {
                    // propergate lhs type
                    node.DeterminedType = info.TypeInfo = node.Right?.DeterminedType ?? node.Left?.DeterminedType;
                }
                else if (info.SubType == TokenType.IndexerNode)
                {
                    // propergate lhs type
                    node.DeterminedType = info.TypeInfo = node.Left.DeterminedType.GetUnderlyingElementType();
                }
                else if (info.SubType == TokenType.InvokeMethod)
                {
                    // if left side node type is not ExtendedMethodPlaceholder
                    if (node.Left?.DeterminedType != typeof(ExtendedMethodPlaceholder))
                    {
                        lhs.Last(x => x.Position == node.Left.Right.Token.Position).Type = SyntaxType.Error;
                    }
                    else
                    {
                        var ie = node.Left.Left.DeterminedType; // target IE<T>
                        var methods = GetMethodInfo(ie, node.Left.Right.Token.Value).ToArray();
                        var args = ExpandArguments(node.Right).ToArray();
                        if (methods.All(m => m.GetParameters().Length != args.Length + 1))
                        {
                            lhs.Last(x => x.Position == node.Left.Right.Token.Position).Type = SyntaxType.Error;
                        }
                        else
                        {
                            var elementType = ie.GetUnderlyingElementType();
                            foreach (var arg in args)
                            {
                                if (arg.Token.Type != TokenType.Lambda) continue;
                                // update lambda related ast
                                arg.Left.DeterminedType = elementType;
                                var lambda = HighlightOne(arg.Right, null, arg.Left).ToList();
                                foreach (var s in lambda)
                                {
                                    var z = rhs.First(x => x.Position == s.Position);
                                    z.TypeInfo = s.TypeInfo;
                                    z.Type = s.Type;
                                }
                                rhs.Last(x => x.Position == arg.Left.Token.Position).Type = SyntaxType.Identifier;
                                arg.DeterminedType = GetDelegateType(elementType, arg.Right.DeterminedType);
                            }
                            // fill generic parameter
                            var method = Helper.FindPreferredMethod(node.Left.Right.Token.Value,
                                new[] { ie.GetUnderlyingEnumerableType() }.Concat(args.Select(x => x.DeterminedType)).ToArray());
                            info.TypeInfo = node.DeterminedType = method?.ReturnType;
                        }
                    }
                }
                else
                {
                    node.DeterminedType = info.TypeInfo = typeof(bool);
                }
            }

            // build enumerable
            foreach (var i in lhs) yield return i;
            yield return info;
            foreach (var i in rhs) yield return i;
        }

        private static Type GetDelegateType(Type elementType, Type determinedType)
        {
            return typeof(Func<,>).MakeGenericType(elementType, determinedType);
        }

        private static IEnumerable<ASTNode> ExpandArguments(ASTNode node)
        {
            if (node == null) yield break;
            if (node.Token.Type == TokenType.Nop) { yield break; }
            if (node.Token.Type == TokenType.ArrayDelimiter)
            {
                foreach (var n in ExpandArguments(node.Left)) { yield return n; }
                foreach (var n in ExpandArguments(node.Right)) { yield return n; }
                yield break;
            }
            yield return node;
        }

        private static IEnumerable<SyntaxInfo> SyntaxHighlightAST(IEnumerable<ASTNode> nodes)
        {
            if (nodes == null) yield break;
            foreach (var i in nodes.SelectMany(n => HighlightOne(n)))
            {
                yield return i;
            }
        }

        public static IEnumerable<SyntaxInfo> SyntaxHighlight(IEnumerable<CompilerToken> tokenList)
        {
            var infos = SyntaxHighlightAST(ASTBuilder.Generate(tokenList)).OrderBy(x => x.Position).ToArray(); //ASTBuilder.Generate(tokenList).Select(HighlightOne).ToArray();
            return infos;
        }
    }
}
