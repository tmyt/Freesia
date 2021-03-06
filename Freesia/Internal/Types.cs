﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Freesia.Internal.Reflection;
using Freesia.Types;

namespace Freesia.Internal.Types
{
    internal class ASTNode : IASTNode
    {
        public CompilerToken Token { get; set; }
        public ASTNode Left { get; set; }
        public ASTNode Right { get; set; }
        public Type DeterminedType { get; set; }

        public ASTNode() { }
        public ASTNode(CompilerToken token)
        {
            this.Token = token;
        }

        public override string ToString()
        {
            return Dump();
        }

        internal string Dump()
        {
            return $"<{Left?.Dump()} {Token} {Right?.Dump()}>";
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
            {TokenType.InvokeMethod, 10 },
            {TokenType.Not, 20},
            {TokenType.UnaryPlus, 20},
            {TokenType.UnaryMinus, 20},
            {TokenType.Multiply, 21},
            {TokenType.Divide, 21},
            {TokenType.Modulo, 21},
            {TokenType.Plus, 22},
            {TokenType.Minus, 22},
            {TokenType.ShiftLeft, 23},
            {TokenType.ShiftRight, 23},
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
            {TokenType.Lambda, 60 },
            {TokenType.ArrayDelimiter, 70 },
        };

        public static Dictionary<TokenType, Associativity> Associativity = new Dictionary<TokenType, Associativity>
        {
            {TokenType.PropertyAccess, Types.Associativity.LeftToRight },
            {TokenType.InvokeMethod, Types.Associativity.LeftToRight },
            {TokenType.Not, Types.Associativity.RightToLeft},
            {TokenType.UnaryPlus, Types.Associativity.RightToLeft},
            {TokenType.UnaryMinus, Types.Associativity.RightToLeft},
            {TokenType.Multiply, Types.Associativity.LeftToRight},
            {TokenType.Divide, Types.Associativity.LeftToRight},
            {TokenType.Modulo, Types.Associativity.LeftToRight},
            {TokenType.Plus, Types.Associativity.LeftToRight},
            {TokenType.Minus, Types.Associativity.LeftToRight},
            {TokenType.ShiftLeft, Types.Associativity.LeftToRight},
            {TokenType.ShiftRight, Types.Associativity.LeftToRight},
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
            {TokenType.Lambda, Types.Associativity.RightToLeft} ,
            {TokenType.ArrayDelimiter, Types.Associativity.LeftToRight },
        };
    }
}
