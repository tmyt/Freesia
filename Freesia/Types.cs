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
            return $"{Type}({SubType}): {Value}";
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

        public bool IsOperator => this.Type.IsOperator();

        public bool IsBooleanOperator => this.Type.IsBooleanOperator();

        public bool IsUnaryOperator => this.Type.IsUnaryOperator();

        public bool IsSymbol => this.Type.IsSymbol();

        public bool IsConstant => this.Type.IsConstant();
    }

    public enum SyntaxType
    {
        Constant,
        Operator,
        Keyword,
        Argument,
        Identifier,
        String,
        Error,
    }

    public enum TokenType
    {
        UnaryPlus,
        UnaryMinus,
        Plus,
        Minus,
        Multiply,
        Divide,
        Modulo,
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
        LambdaParameter,
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
        Error,
    }

    public static class TokenTypeExtensions
    {
        public static bool IsOperator(this TokenType that)
        {
            return Operators.Priority.ContainsKey(that);
        }

        public static bool IsBooleanOperator(this TokenType that)
        {
            return that.IsOperator()
                   && that != TokenType.PropertyAccess
                   && that != TokenType.InvokeMethod
                   && that != TokenType.UnaryPlus
                   && that != TokenType.UnaryMinus
                   && that != TokenType.Plus
                   && that != TokenType.Minus
                   && that != TokenType.Multiply
                   && that != TokenType.Divide
                   && that != TokenType.Modulo;
        }

        public static bool IsUnaryOperator(this TokenType that)
        {
            return that == TokenType.UnaryPlus
                   || that == TokenType.UnaryMinus
                   || that == TokenType.Not;
        }

        public static bool IsSymbol(this TokenType that)
        {
            return that == TokenType.Symbol
                   || that == TokenType.String
                   || that == TokenType.Double
                   || that == TokenType.Long
                   || that == TokenType.ULong
                   || that == TokenType.Bool
                   || that == TokenType.Null;
        }

        public static bool IsConstant(this TokenType that)
        {
            return that == TokenType.String
                   || that == TokenType.Double
                   || that == TokenType.Long
                   || that == TokenType.ULong
                   || that == TokenType.Bool
                   || that == TokenType.Null;
        }
    }
}
