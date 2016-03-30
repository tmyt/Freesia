using Freesia.Internal.Types;

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

        public bool IsOperator()
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

        public bool IsConstant()
        {
            return this.Type == TokenType.String
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
        PropertyAccess,
        InvokeMethod,
    }
}
