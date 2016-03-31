using System;
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
        public Type TypeInfo { get; set; }

        public SyntaxInfo() { }

        public SyntaxInfo(CompilerToken token, SyntaxType type)
        {
            this.Type = type;
            this.SubType = token.Type;
            this.Value = token.Value;
            this.Position = token.Position;
            this.Length = token.Length;
        }

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

        public bool IsOperator => Operators.Priority.ContainsKey(this.Type);

        public bool IsSymbol => this.Type == TokenType.Symbol
                                || this.Type == TokenType.String
                                || this.Type == TokenType.Double
                                || this.Type == TokenType.Long
                                || this.Type == TokenType.ULong
                                || this.Type == TokenType.Bool
                                || this.Type == TokenType.Null;

        public bool IsConstant => this.Type == TokenType.String
                                  || this.Type == TokenType.Double
                                  || this.Type == TokenType.Long
                                  || this.Type == TokenType.ULong
                                  || this.Type == TokenType.Bool
                                  || this.Type == TokenType.Null;
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
        ArrayNode,
        IndexerStart,
        IndexerEnd,
        IndexerNode,
        Lambda,
        PropertyAccess,
        InvokeMethod,
        Symbol,
        String,
        Double,
        Long,
        ULong,
        Bool,
        Null,
        Nop,
    }
}
