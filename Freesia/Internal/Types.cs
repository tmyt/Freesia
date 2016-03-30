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
            return $"{Left?.Dump()} {Token} {Right?.Dump()}";
        }
    }

    internal class ArrayProperty
    {
        public string PropName { get; set; }
        public string ArrayAccessor { get; set; }
        public int Index { get; set; }
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
            {TokenType.PropertyAccess, Internal.Types.Associativity.LeftToRight },
            {TokenType.Not, Internal.Types.Associativity.RightToLeft},
            {TokenType.LessThan, Internal.Types.Associativity.LeftToRight},
            {TokenType.GreaterThan, Internal.Types.Associativity.LeftToRight},
            {TokenType.LessThanEquals, Internal.Types.Associativity.LeftToRight},
            {TokenType.GreaterThanEquals, Internal.Types.Associativity.LeftToRight},
            {TokenType.Equals, Internal.Types.Associativity.LeftToRight},
            {TokenType.EqualsI, Internal.Types.Associativity.LeftToRight},
            {TokenType.NotEquals, Internal.Types.Associativity.LeftToRight},
            {TokenType.NotEqualsI, Internal.Types.Associativity.LeftToRight},
            {TokenType.Regexp, Internal.Types.Associativity.LeftToRight},
            {TokenType.NotRegexp, Internal.Types.Associativity.LeftToRight},
            {TokenType.Contains, Internal.Types.Associativity.LeftToRight},
            {TokenType.ContainsI, Internal.Types.Associativity.LeftToRight},
            {TokenType.NotContains, Internal.Types.Associativity.LeftToRight},
            {TokenType.NotContainsI, Internal.Types.Associativity.LeftToRight},
            {TokenType.And, Internal.Types.Associativity.LeftToRight},
            {TokenType.Or, Internal.Types.Associativity.LeftToRight},
            {TokenType.Lambda, Internal.Types.Associativity.RightToLeft }
        };
    }

}
