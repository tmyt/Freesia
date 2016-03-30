using System.Collections.Generic;
using Freesia.Internal.Types;
using Freesia.Types;

namespace Freesia.Internal
{
    internal class ASTBuilder
    {
        private static ASTNode MakeAst(CompilerToken op, ref Stack<ASTNode> values)
        {
            var rhs = values.Pop();
            var lhs = values.Pop();
            return op.Type == TokenType.Not
                ? new ASTNode { Token = op, Left = rhs }
                : new ASTNode { Token = op, Left = lhs, Right = rhs };
        }

        public static ASTNode Generate(IEnumerable<CompilerToken> list)
        {
            var ops = new Stack<CompilerToken>();
            var values = new Stack<ASTNode>();
            CompilerToken p1, p2 = null;
            var inArray = false;
            foreach (var token in list)
            {
                p1 = p2;
                p2 = token;
                // skip ArrayDelimiter token
                if (inArray && token.Type == TokenType.ArrayDelimiter)
                    continue;
                // break Array parsing
                if (token.Type == TokenType.ArrayEnd) inArray = false;
                // check ArrayDelimiter
                if (inArray && p1 != null &&
                    (p1.Type != TokenType.ArrayStart && p1.Type != TokenType.ArrayDelimiter))
                    throw new ParseException("Array elements must be delimitered ','.", -1);
                // enter Array parsing
                if (token.Type == TokenType.ArrayStart) inArray = true;
                // correct token
                if (token.IsSymbol() || token.Type == TokenType.ArrayStart)
                {
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
                        if (arrayNode.Right == null) { arrayNode.Right = node; }
                        else if (arrayNode.Left == null) { arrayNode.Left = node; }
                        else
                        {
                            arrayNode.Token = new CompilerToken { Type = TokenType.ArrayDelimiter, Value = ",", Length = 1 };
                            arrayNode = new ASTNode { Right = arrayNode, Left = node };
                        }
                        values.Pop();
                        node = values.Peek();
                    }
                    arrayNode.Token = new CompilerToken { Type = TokenType.ArrayNode, Value = "{}", Length = 2, Position = node.Token.Position };
                    if (arrayNode.Left == null)
                    {
                        arrayNode.Left = arrayNode.Right;
                        arrayNode.Right = null;
                    }
                    values.Pop();
                    values.Push(arrayNode);
                    continue;
                }
                // found ')'
                if (token.Type == TokenType.CloseBracket)
                {
                    while (ops.Peek().Type != TokenType.OpenBracket)
                    {
                        values.Push(MakeAst(ops.Pop(), ref values));
                    }
                    ops.Pop();
                    continue;
                }
                // found '('
                if (token.Type == TokenType.OpenBracket)
                {
                    ops.Push(token);
                    continue;
                }
                // enumerate pushed ops
                while (ops.Count != 0)
                {
                    var t = ops.Peek();
                    if (t.Type == TokenType.OpenBracket) break;
                    if (Operators.Priority[t.Type] >= Operators.Priority[token.Type])
                        break;
                    values.Push(MakeAst(ops.Pop(), ref values));
                }
                ops.Push(token);
                // add pseudo value
                if (token.Type == TokenType.Not)
                {
                    values.Push(new ASTNode(new CompilerToken { Type = TokenType.Nop }));
                }
            }
            // take all ops
            while (ops.Count != 0)
            {
                values.Push(MakeAst(ops.Pop(), ref values));
            }
            // this is AST
            return values.Pop();
        }
    }
}
