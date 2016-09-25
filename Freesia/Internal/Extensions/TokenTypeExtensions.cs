using Freesia.Internal.Types;
using Freesia.Types;

namespace Freesia.Internal.Extensions
{
    internal static class TokenTypeExtensions
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
                   && that != TokenType.Modulo
                   && that != TokenType.ShiftLeft
                   && that != TokenType.ShiftRight;
        }

        public static bool IsUnaryOperator(this TokenType that)
        {
            return that == TokenType.UnaryPlus
                   || that == TokenType.UnaryMinus
                   || that == TokenType.Not;
        }

        public static bool IsBinaryOperator(this TokenType that)
        {
            return that.IsOperator() && !that.IsUnaryOperator();
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
