﻿using System;
using Freesia.Internal.Extensions;

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

        public bool IsOpenBrackets => this.Type.IsOpenBrackets();
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
        ShiftLeft,
        ShiftRight,
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
}
