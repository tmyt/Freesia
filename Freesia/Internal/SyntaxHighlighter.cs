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
                case TokenType.LambdaParameter:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Keyword, Value = t.Value, TypeInfo = null /* to be determin */ };
                case TokenType.Error:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Error, Value = t.Value ?? "", TypeInfo = null };
                default:
                    throw new ParseException("#-1", t.Position);
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

        private static IEnumerable<MethodInfo> GetMethodInfo(string name)
        {
            return Helper.GetEnumerableExtendedMethodInfos().Where(m => m.Name.ToLowerInvariant() == name);
        }

        private static void UpdateASTNodeType(ASTNode node, Type parentNodeType = null, ASTNode lambdaArg = null)
        {
            if (node == null) return;
            if (node.Token.Type == TokenType.Nop) return;
            parentNodeType = parentNodeType ?? typeof(T);

            // determine operator node type
            var info = TranslateSyntaxInfo(node.Token);
            // determine identifier type
            if (node.Token.Type == TokenType.Error) return;
            if (node.Token.Type == TokenType.Symbol)
            {
                node.DeterminedType = GetPropertyType(parentNodeType, node.Token.Value);
                if (lambdaArg != null && node.Token.Value == lambdaArg.Token.Value)
                {
                    node.DeterminedType = lambdaArg.DeterminedType;
                    node.Token.Type = TokenType.LambdaParameter;
                }
                if (node.DeterminedType == null)
                {
                    node.DeterminedType = IsExtendedMethod(parentNodeType, node.Token.Value.ToLowerInvariant())
                        ? typeof(ExtendedMethodPlaceholder) : null;
                }
                if (node.DeterminedType == null)
                {
                    // error node
                    node.DeterminedType = info.TypeInfo;
                }
                return;
            }

            // process left node (and update ``node.Left.DeterminedType'' value)
            UpdateASTNodeType(node.Left, parentNodeType, lambdaArg);
            // process right node
            UpdateASTNodeType(node.Right, node.Left?.DeterminedType, lambdaArg);

            // info is no Operator
            if (info.Type != SyntaxType.Operator)
            {
                node.DeterminedType = info.TypeInfo;
                return;
            }
            if (node.Token.Type == TokenType.PropertyAccess)
            {
                // propergate lhs type
                node.DeterminedType = node.Right?.DeterminedType ?? node.Left?.DeterminedType;
            }
            else if (node.Token.Type == TokenType.IndexerNode)
            {
                // propergate lhs type
                node.DeterminedType = node.Left.DeterminedType.GetUnderlyingElementType();
            }
            else if (node.Token.Type == TokenType.InvokeMethod)
            {
                // if left side node type is not ExtendedMethodPlaceholder
                if (node.Left?.DeterminedType != typeof(ExtendedMethodPlaceholder)) {/* error */return; }
                var ie = node.Left.Left.DeterminedType; // target IE<T>
                var methods = GetMethodInfo(node.Left.Right.Token.Value.ToLowerInvariant()).ToArray();
                var args = ExpandArguments(node.Right).ToArray();
                if (methods.All(m => m.GetParameters().Length != args.Length + 1)) {/* error */return; }
                var elementType = ie.GetUnderlyingElementType();
                foreach (var arg in args)
                {
                    if (arg.Token.Type != TokenType.Lambda) continue;
                    // update lambda related ast
                    arg.Left.DeterminedType = elementType;
                    arg.Left.Token.Type = TokenType.LambdaParameter;
                    UpdateASTNodeType(arg.Right, null, arg.Left);
                    arg.DeterminedType = GetDelegateType(elementType, arg.Right.DeterminedType);
                }
                // fill generic parameter
                var method = Helper.FindPreferredMethod(node.Left.Right.Token.Value,
                    new[] { ie.GetUnderlyingEnumerableType() }.Concat(args.Select(x => x.DeterminedType)).ToArray());
                node.DeterminedType = method?.ReturnType;
            }
            else
            {
                node.DeterminedType = typeof(bool);
            }
        }

        private static IEnumerable<SyntaxInfo> HighlightOne(ASTNode node)
        {
            if (node == null) yield break;
            if (node.Token.Type == TokenType.Nop) yield break;
            var info = TranslateSyntaxInfo(node.Token);
            // determine identifier type
            info.TypeInfo = node.DeterminedType;
            if (info.Type == SyntaxType.Identifier && node.DeterminedType == null)
            {
                info.Type = SyntaxType.Error;
            }
            // process left node (and update ``node.Left.DeterminedType'' value)
            var lhs = HighlightOne(node.Left).ToList();
            // process right node
            var rhs = HighlightOne(node.Right).ToList();
            // build enumerable
            foreach (var i in lhs) yield return i;
            yield return info;
            foreach (var i in rhs) yield return i;
        }

        private static Type GetDelegateType(Type elementType, Type determinedType)
        {
            if (determinedType == null) return null;
            return typeof(Func<,>).MakeGenericType(elementType, determinedType);
        }

        private static IEnumerable<ASTNode> ExpandArguments(ASTNode node)
        {
            if (node == null) yield break;
            if (node.Token.Type == TokenType.CloseBracket) { yield break; }
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
            foreach (var node in nodes)
            {
                UpdateASTNodeType(node);
                foreach (var i in HighlightOne(node)) yield return i;
            }
        }

        public static IEnumerable<SyntaxInfo> SyntaxHighlight(IEnumerable<IASTNode> ast)
        {
            var infos = SyntaxHighlightAST(ast.Cast<ASTNode>()).OrderBy(x => x.Position).ToArray();
            return infos;
        }
    }
}
