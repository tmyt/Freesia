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
        Lambda,
        IndexerStart,
        IndexerEnd,
        IndexerNode,
        PropertyAccess
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
    }
}
