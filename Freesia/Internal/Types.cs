using System.Collections.Generic;
using Freesia.Types;

namespace Freesia.Internal.Types
{
    internal class ASTNode
    {
        public CompilerToken Token { get; set; }
        public ASTNode Left { get; set; }
        public ASTNode Right { get; set; }

        public ASTNode() { }
        public ASTNode(CompilerToken token)
        {
            this.Token = token;
        }

        internal string Dump()
        {
            return $"({Left?.Dump()} {Token} {Right?.Dump()})";
        }
    }
    
    internal enum Associativity
    {
        LeftToRight,
        RightToLeft
    }

    internal static class Operators
    {
        public static Dictionary<TokenType, byte> Priority = new Dictionary<TokenType, byte>
        {
            {TokenType.PropertyAccess, 10 },
            {TokenType.Not, 20},
            {TokenType.LessThan, 30},
            {TokenType.GreaterThan, 30},
            {TokenType.LessThanEquals, 30},
            {TokenType.GreaterThanEquals, 30},
            {TokenType.Equals, 40},
            {TokenType.EqualsI, 40},
            {TokenType.NotEquals, 40},
            {TokenType.NotEqualsI, 40},
            {TokenType.Regexp, 40},
            {TokenType.NotRegexp, 40},
            {TokenType.Contains, 40},
            {TokenType.ContainsI, 40},
            {TokenType.NotContains, 40},
            {TokenType.NotContainsI, 40},
            {TokenType.And, 50},
            {TokenType.Or, 50},
            {TokenType.Lambda, 100 }
        };

        public static Dictionary<TokenType, Associativity> Associativity = new Dictionary<TokenType, Associativity>
        {
            {TokenType.PropertyAccess, Types.Associativity.LeftToRight },
            {TokenType.Not, Types.Associativity.RightToLeft},
            {TokenType.LessThan, Types.Associativity.LeftToRight},
            {TokenType.GreaterThan, Types.Associativity.LeftToRight},
            {TokenType.LessThanEquals, Types.Associativity.LeftToRight},
            {TokenType.GreaterThanEquals, Types.Associativity.LeftToRight},
            {TokenType.Equals, Types.Associativity.LeftToRight},
            {TokenType.EqualsI, Types.Associativity.LeftToRight},
            {TokenType.NotEquals, Types.Associativity.LeftToRight},
            {TokenType.NotEqualsI, Types.Associativity.LeftToRight},
            {TokenType.Regexp, Types.Associativity.LeftToRight},
            {TokenType.NotRegexp, Types.Associativity.LeftToRight},
            {TokenType.Contains, Types.Associativity.LeftToRight},
            {TokenType.ContainsI, Types.Associativity.LeftToRight},
            {TokenType.NotContains, Types.Associativity.LeftToRight},
            {TokenType.NotContainsI, Types.Associativity.LeftToRight},
            {TokenType.And, Types.Associativity.LeftToRight},
            {TokenType.Or, Types.Associativity.LeftToRight},
            {TokenType.Lambda, Types.Associativity.RightToLeft }
        };
    }

}
