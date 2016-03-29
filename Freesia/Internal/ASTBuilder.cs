using System.Collections.Generic;
using Freesia.Internal.Types;
using Freesia.Types;

namespace Freesia.Internal
{
    internal class ASTBuilder
    {
        public static ASTNode Generate(IEnumerable<CompilerToken> list)
        {
            var work = new Stack<ASTNode>();
            foreach (var token in ReorderTokens(list))
            {
                // 配列の終端見つけました！
                if (token.Type == TokenType.ArrayEnd)
                {
                    var node = work.Peek();
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
                        work.Pop();
                        node = work.Peek();
                    }
                    arrayNode.Token = new CompilerToken { Type = TokenType.ArrayNode, Value = "{}", Length = 2 };
                    if (arrayNode.Left == null)
                    {
                        arrayNode.Left = arrayNode.Right;
                        arrayNode.Right = null;
                    }
                    work.Pop();
                    work.Push(arrayNode);
                    continue;
                }
                if (token.IsOperand())
                {
                    // 2個とってExpressionにする
                    var rhs = work.Pop();
                    var lhs = work.Pop();
                    work.Push(token.Type == TokenType.Not
                        ? new ASTNode { Token = token, Left = rhs }
                        : new ASTNode { Token = token, Left = lhs, Right = rhs });
                    continue;
                }
                work.Push(new ASTNode { Token = token });
            }
            if (work.Count != 1) throw new ParseException("Syntax error.", -1);
            return work.Peek(); // AST comes here!
        }

        private static IEnumerable<CompilerToken> ReorderTokens(IEnumerable<CompilerToken> list)
        {
            var work = new Stack<CompilerToken>();
            CompilerToken p1, p2 = null;
            var inArray = false;
            foreach (var token in list)
            {
                p1 = p2;
                p2 = token;
                if (inArray && token.Type == TokenType.ArrayDelimiter)
                    continue;
                if (token.Type == TokenType.ArrayEnd) inArray = false;
                if (inArray && p1 != null &&
                    (p1.Type != TokenType.ArrayStart && p1.Type != TokenType.ArrayDelimiter))
                    throw new ParseException("Array elements must be delimitered ','.", -1);
                if (token.Type == TokenType.ArrayStart) inArray = true;
                if (token.IsSymbol() || token.Type == TokenType.ArrayStart || token.Type == TokenType.ArrayEnd)
                {
                    yield return token;
                    continue;
                }
                if (token.Type == TokenType.CloseBracket)
                {
                    while (work.Peek().Type != TokenType.OpenBracket)
                    {
                        yield return work.Pop();
                    }
                    work.Pop();
                    continue;
                }
                if (token.Type == TokenType.OpenBracket)
                {
                    work.Push(token);
                    continue;
                }
                while (work.Count != 0)
                {
                    var t = work.Peek();
                    if (t.Type == TokenType.OpenBracket) break;
                    if (Operators.Priority[t.Type] >= Operators.Priority[token.Type])
                        break;
                    yield return work.Pop();
                }
                work.Push(token);
                if (token.Type == TokenType.Not)
                {
                    yield return new CompilerToken { Type = TokenType.Nop };
                }
            }
            while (work.Count != 0) yield return work.Pop();
        }
    }
}
