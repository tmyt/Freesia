using System;
using System.Collections.Generic;
using Freesia.Internal.Types;
using Freesia.Types;

namespace Freesia.Internal
{
    internal class ASTBuilder
    {
        private static ASTNode NopNode(CompilerToken parentNode)
        {
            return new ASTNode(new CompilerToken { Length = 0, Position = parentNode.Position, Type = TokenType.Nop, Value = "" });
        }

        private static ASTNode MakeAst(CompilerToken op, ref Stack<ASTNode> values)
        {
            var rhs = values.Count > 0 ? values.Pop() : NopNode(op);
            var lhs = values.Count > 0 ? values.Pop() : NopNode(op);
            return op.Type == TokenType.Not
                ? new ASTNode { Token = op, Left = rhs }
                : new ASTNode { Token = op, Left = lhs, Right = rhs };
        }

        private static ASTNode MakeErrorNode()
        {
            return new ASTNode(new CompilerToken { Type = TokenType.Nop, Value = "*ERROR*", Length = 0, Position = -1 });
        }

        private static IEnumerable<ASTNode> GenerateInternal(IEnumerable<CompilerToken> list)
        {
            var ops = new Stack<CompilerToken>();
            var values = new Stack<ASTNode>();
            var trees = new List<ASTNode>();
            CompilerToken p1, p2 = null;
            var inArray = false;
            var needRhs = false;
            foreach (var _token in list)
            {
                var token = _token; // make writeable
                p1 = p2;
                p2 = token;
                // take symbol continuasly, it's seems error. try to recovery it.
                if (p2.IsSymbol && (p1?.IsSymbol).GetValueOrDefault())
                {
                    // pop all ops
                    while (ops.Count != 0)
                    {
                        values.Push(MakeAst(ops.Pop(), ref values));
                    }
                    trees.Add(values.Pop());
                    p1 = null;
                }
                // skip ArrayDelimiter token
                if (inArray && token.Type == TokenType.ArrayDelimiter)
                {
                    values.Push(new ASTNode(new CompilerToken { Type = TokenType.ArrayDelimiter, Value = ",", Length = 1, Position = token.Position }));
                    continue;
                }
                // break Array parsing
                if (token.Type == TokenType.ArrayEnd) inArray = false;
                // check ArrayDelimiter
                if (inArray && p1 != null &&
                    (p1.Type != TokenType.ArrayStart && p1.Type != TokenType.ArrayDelimiter))
                    throw new ParseException("Array elements must be delimitered ','.", -1);
                // enter Array parsing
                if (token.Type == TokenType.ArrayStart) inArray = true;
                // check arg needed
                if (token.Type == TokenType.PropertyAccess) needRhs = true;
                // correct token
                if (token.IsSymbol || token.Type == TokenType.ArrayStart || token.Type == TokenType.IndexerStart)
                {
                    needRhs = false;
                    values.Push(new ASTNode(token));
                    continue;
                }
                // generate AST for Array
                if (token.Type == TokenType.ArrayEnd)
                {
                    var node = values.Peek();
                    var arrayNode = new ASTNode();
                    while (node.Token.Type != TokenType.ArrayStart)
                    {
                        if (node.Token.Type == TokenType.ArrayDelimiter)
                        {
                            if (arrayNode.Token == null)
                            {
                                arrayNode.Token = node.Token;
                            }
                            else
                            {
                                arrayNode = new ASTNode { Right = arrayNode, Token = node.Token };
                            }
                        }
                        else if (arrayNode.Right == null) { arrayNode.Right = node; }
                        else if (arrayNode.Left == null) { arrayNode.Left = node; }
                        values.Pop();
                        node = values.Peek();
                    }
                    arrayNode.Token = new CompilerToken { Type = TokenType.ArrayNode, Value = "{", Length = 1, Position = node.Token.Position };
                    if (arrayNode.Left == null)
                    {
                        arrayNode.Left = arrayNode.Right;
                        arrayNode.Right = null;
                    }
                    values.Pop();
                    values.Push(arrayNode);
                    continue;
                }
                // generate AST for Indexer
                if (token.Type == TokenType.IndexerEnd)
                {
                    var node = values.Peek();
                    var indexerNode = new ASTNode();
                    var valueTaken = false;
                    while (node.Token.Type != TokenType.IndexerStart)
                    {
                        if (valueTaken) throw new ParseException("Indexer should be one token.", node.Token.Position);
                        valueTaken = true;
                        indexerNode.Right = node;
                        values.Pop();
                        node = values.Peek();
                    }
                    indexerNode.Token = new CompilerToken { Type = TokenType.IndexerNode, Value = "[", Length = 1, Position = node.Token.Position };
                    values.Pop();
                    indexerNode.Left = values.Pop();
                    values.Push(indexerNode);
                    continue;
                }
                // found ')'
                if (token.Type == TokenType.CloseBracket)
                {
                    var nopBracket = p1?.Type == TokenType.OpenBracket;
                    while (ops.Peek().Type != TokenType.OpenBracket)
                    {
                        values.Push(MakeAst(ops.Pop(), ref values));
                    }
                    ops.Pop();
                    // for zero arguments
                    if (nopBracket && ops.Count > 0 && ops.Peek().Type == TokenType.InvokeMethod)
                        values.Push(new ASTNode(new CompilerToken { Type = TokenType.Nop }));
                    continue;
                }
                // found '('
                if (token.Type == TokenType.OpenBracket)
                {
                    if (p1 != null && (p1.Type == TokenType.Symbol
                        || p1.Type == TokenType.PropertyAccess
                        || p1.Type == TokenType.IndexerNode))
                    {
                        token = new CompilerToken { Type = TokenType.InvokeMethod, Value = "(", Length = 1, Position = token.Position };
                    }
                    else
                    {
                        ops.Push(token);
                        continue;
                    }
                }
                // enumerate pushed ops
                while (ops.Count != 0)
                {
                    var t = ops.Peek();
                    if (t.Type == TokenType.OpenBracket) break;
                    if ((Operators.Associativity[t.Type] == Associativity.LeftToRight &&
                         Operators.Priority[t.Type] > Operators.Priority[token.Type])
                        ||
                        (Operators.Associativity[t.Type] == Associativity.RightToLeft &&
                         Operators.Priority[t.Type] >= Operators.Priority[token.Type]))
                    {
                        break;
                    }
                    values.Push(MakeAst(ops.Pop(), ref values));
                }
                ops.Push(token);
                // add '(' for MethodInvoke
                if (token.Type == TokenType.InvokeMethod)
                    ops.Push(new CompilerToken { Type = TokenType.OpenBracket, Value = "(", Length = 1, Position = token.Position });
                // add pseudo value
                if (token.Type == TokenType.Not)
                    values.Push(new ASTNode(new CompilerToken { Type = TokenType.Nop }));
            }
            // return empty ast
            if (values.Count == 0 && ops.Count == 0)
            {
                return new[] { MakeErrorNode() };
            }
            // push null if needed
            if (needRhs)
            {
                values.Push(null);
            }
            // take all ops
            while (ops.Count != 0)
            {
                if (ops.Peek().Type == TokenType.OpenBracket) { ops.Pop(); continue; }
                if (values.Count == 1) { values.Push(null); }
                values.Push(MakeAst(ops.Pop(), ref values));
            }
            trees.Add(values.Pop());
            // this is AST
            return trees;
        }

        public static IEnumerable<ASTNode> Generate(IEnumerable<CompilerToken> list)
        {
            try
            {
                return GenerateInternal(list);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
