using System.Collections.Generic;

namespace Freesia.Types
{
    public class SyntaxInfo
    {
        public SyntaxType Type { get; set; }
        public TokenType SubType { get; set; }
        public string Value { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }
    }

    public class CompilerToken
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }

        public bool IsOperand()
        {
            return Operators.Priority.ContainsKey(this.Type);
        }

        public bool IsSymbol()
        {
            return this.Type == TokenType.Symbol
                   || this.Type == TokenType.String
                   || this.Type == TokenType.Double
                   || this.Type == TokenType.Long
                   || this.Type == TokenType.ULong
                   || this.Type == TokenType.Bool
                   || this.Type == TokenType.Null;
        }
    }

    public enum SyntaxType
    {
        Constant,
        ArrayArgs,
        Operator,
        Keyword,
        Identifier,
        String,
        Error
    }

    public enum TokenType
    {
        Equals,
        EqualsI,
        NotEquals,
        NotEqualsI,
        Regexp,
        NotRegexp,
        Contains,
        ContainsI,
        NotContains,
        NotContainsI,
        And,
        Or,
        Not,
        LessThan,
        GreaterThan,
        LessThanEquals,
        GreaterThanEquals,
        OpenBracket,
        CloseBracket,
        ArrayStart,
        ArrayEnd,
        ArrayDelimiter,
        Symbol,
        String,
        Double,
        Long,
        ULong,
        Bool,
        Null,
        Nop,
        ArrayNode,
        Lambda
    }

    internal static class Operators
    {
        public static Dictionary<TokenType, byte> Priority = new Dictionary<TokenType, byte>
        {
            {TokenType.Not, 0},
            {TokenType.LessThan, 1},
            {TokenType.GreaterThan, 1},
            {TokenType.LessThanEquals, 1},
            {TokenType.GreaterThanEquals, 1},
            {TokenType.Equals, 2},
            {TokenType.EqualsI, 2},
            {TokenType.NotEquals, 2},
            {TokenType.NotEqualsI, 2},
            {TokenType.Regexp, 2},
            {TokenType.NotRegexp, 2},
            {TokenType.Contains, 2},
            {TokenType.ContainsI, 2},
            {TokenType.NotContains, 2},
            {TokenType.NotContainsI, 2},
            {TokenType.And, 3},
            {TokenType.Or, 3},
            {TokenType.Lambda, 99 }
        };
    }
}
