using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Freesia.Internal;
using Freesia.Internal.Types;
using Freesia.Types;

namespace Freesia
{
    public class FilterCompiler<T>
    {
        private class UserFunctionTypePlaceholder { }

        private delegate Expression BinaryExpressionBuilder(Expression lhs, Expression rhs);
        private delegate Expression UnaryExpressionBuilder(Expression expr);

        private readonly ParameterExpression _rootParameter = Expression.Parameter(typeof(T), "status");
        private readonly Dictionary<string, ParameterExpression> _env = new Dictionary<string, ParameterExpression>();

        private static Dictionary<string, Func<T, bool>> _functions;
        public static Dictionary<string, Func<T, bool>> Functions => _functions ?? (_functions = new Dictionary<string, Func<T, bool>>());

        public static string UserFunctionNamespace { get; set; }

        internal FilterCompiler()
        {
        }

        private ASTNode Tokenize(string text)
        {
            var tokenizer = new Tokenizer(text);
            return ASTBuilder.Generate(tokenizer.Parse());
        }

        private object CompileOne(ASTNode ast)
        {
            if (ast.Token.Type == TokenType.ArrayNode)
            {
                var items = new List<object>();
                if (ast.Left != null) items.Add(ast.Left.Token);
                var current = ast.Right;
                while (current != null)
                {
                    if (current.Token.Type == TokenType.ArrayDelimiter)
                    {
                        items.Add(current.Left.Token);
                        current = current.Right;
                        continue;
                    }
                    items.Add(current.Token);
                    break;
                }
                return items.ToArray();
            }
            if (!ast.Token.IsOperand())
            {
                return ast.Token;
            }
            if (ast.Token.Type == TokenType.Lambda)
            {
                // Parse for Lambda
                return MakeLambdaExpression(typeof(string), ast.Left.Token, ast.Right);
            }
            if (ast.Token.Type == TokenType.Not)
            {
                // Here is Unary
                return MakeUnaryExpression(Expression.Not, CompileOne(ast.Left));
            }
            var lhs = CompileOne(ast.Left);
            var rhs = CompileOne(ast.Right);
            // 型キャスト
            if (IsConstant(lhs) || IsConstant(rhs))
            {
                bool iscl = IsConstant(lhs), iscr = IsConstant(rhs);
                if (iscl && !iscr)
                    lhs = MakeConvertExpression(rhs, lhs);
                if (!iscl && iscr)
                    rhs = MakeConvertExpression(lhs, rhs);
                if (iscl && iscr) { } // 両方Constとか知らない
            }
            switch (ast.Token.Type)
            {
                case TokenType.Equals:
                    return MakeBinaryExpression(Expression.Equal, lhs, rhs, IsNullValue(lhs) || IsNullValue(rhs));
                case TokenType.EqualsI:
                    return MakeInsensitiveBinaryExpression(Expression.Equal, lhs, rhs);
                case TokenType.NotEquals:
                    return MakeBinaryExpression(Expression.NotEqual, lhs, rhs,
                        IsNullValue(lhs) || IsNullValue(rhs));
                case TokenType.NotEqualsI:
                    return MakeInsensitiveBinaryExpression(Expression.NotEqual, lhs, rhs);
                case TokenType.Regexp:
                    return MakeBinaryExpression(MakeRegexExpression, lhs, rhs);
                case TokenType.NotRegexp:
                    return MakeUnaryExpression(Expression.Not,
                        MakeBinaryExpression(MakeRegexExpression, lhs, rhs));
                case TokenType.Contains:
                    return MakeBinaryExpression(MakeContainsExpression, lhs, rhs);
                case TokenType.ContainsI:
                    return MakeInsensitiveBinaryExpression(MakeContainsExpression, lhs, rhs);
                case TokenType.NotContains:
                    return MakeUnaryExpression(Expression.Not,
                        MakeBinaryExpression(MakeContainsExpression, lhs, rhs));
                case TokenType.NotContainsI:
                    return MakeUnaryExpression(Expression.Not,
                        MakeInsensitiveBinaryExpression(MakeContainsExpression, lhs, rhs));
                case TokenType.And:
                    return MakeBinaryExpression(Expression.AndAlso, lhs, rhs);
                case TokenType.Or:
                    return MakeBinaryExpression(Expression.OrElse, lhs, rhs);
                case TokenType.LessThan:
                    return MakeBinaryExpression(Expression.LessThan, lhs, rhs);
                case TokenType.GreaterThan:
                    return MakeBinaryExpression(Expression.GreaterThan, lhs, rhs);
                case TokenType.LessThanEquals:
                    return MakeBinaryExpression(Expression.LessThanOrEqual, lhs, rhs);
                case TokenType.GreaterThanEquals:
                    return MakeBinaryExpression(Expression.GreaterThanOrEqual, lhs, rhs);
                case TokenType.PropertyAccess:
                    return MakeMemberAccessExpression((Expression)lhs, (CompilerToken)rhs);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Func<T, bool> CompileSyntax(ASTNode ast)
        {
            var root = MakeWrappedExpression(CompileOne(ast));
            if (root.Type != typeof(bool))
            {
                throw new ParseException("Expression couldn't evaluate as boolean.", -1);
            }
            var trycatch = Expression.TryCatch(root,
                Expression.Catch(typeof(Exception), Expression.Constant(false)));
            var expr = Expression.Lambda<Func<T, bool>>(trycatch, _rootParameter);
            return expr.Compile();
        }

        private object MakeConvertExpression(object toValue, object fromValue)
        {
            var type = GetValueType(toValue);
            if (type.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(type) != null)
                type = Nullable.GetUnderlyingType(type);
            if (fromValue is object[])
            {
                var objs = (object[])fromValue;
                for (var i = 0; i < objs.Length; ++i)
                {
                    if (IsConstant(objs[i]))
                        objs[i] = MakeConvertExpression(toValue, objs[i]);
                }
                return objs;
            }
            return Expression.Constant(Convert.ChangeType(GetConstantValue(fromValue), type));
        }

        private Expression MakeUnaryExpression(UnaryExpressionBuilder op, object expr)
        {
            if (expr is object[])
            {
                var objs = (object[])expr;
                var exprs = new Queue<Expression>(objs.Select(o => MakeUnaryExpression(op, o)));
                var e = exprs.Dequeue();
                while (exprs.Count != 0)
                {
                    e = Expression.OrElse(e, exprs.Dequeue());
                }
                return e;
            }
            if (MayNullable(expr))
            {
                // left or right value may be null
                var exprs = new Queue<Expression>();
                if (MayNullable(expr)) exprs.Enqueue(MakeValidation(expr));
                exprs.Enqueue(MakeExpression(expr));
                var e = exprs.Dequeue();
                while (exprs.Count != 0)
                {
                    e = Expression.AndAlso(e, exprs.Dequeue());
                }
                return op(e);
            }
            return op(MakeExpression(expr));
        }

        private Expression MakeBinaryExpression(BinaryExpressionBuilder op, object lhs, object rhs, bool skipNullCheck = false)
        {
            object[] leftArray = lhs as object[], rightArray = rhs as object[];
            if (leftArray != null && rightArray == null)
            {
                var exprs = new Queue<Expression>(leftArray.Select(o => MakeBinaryExpression(op, o, rhs)));
                var e = exprs.Dequeue();
                while (exprs.Count != 0)
                {
                    e = Expression.OrElse(e, exprs.Dequeue());
                }
                return e;
            }
            if (leftArray == null && rightArray != null)
            {
                var exprs = new Queue<Expression>(rightArray.Select(o => MakeBinaryExpression(op, lhs, o)));
                var e = exprs.Dequeue();
                while (exprs.Count != 0)
                {
                    e = Expression.OrElse(e, exprs.Dequeue());
                }
                return e;
            }
            if (leftArray != null) // always true -> && rightArray != null
            {
                var exprs = new Queue<Expression>(leftArray.SelectMany(l => rightArray.Select(r => MakeBinaryExpression(op, l, r))));
                var e = exprs.Dequeue();
                while (exprs.Count != 0)
                {
                    e = Expression.OrElse(e, exprs.Dequeue());
                }
                return e;
            }
            if (!skipNullCheck && (MayNullable(lhs) || MayNullable(rhs)))
            {
                // left or right value may be null
                var exprs = new Queue<Expression>();
                if (MayNullable(lhs))
                {
                    if (Nullable.GetUnderlyingType(GetValueType(lhs)) == typeof(bool))
                        lhs = Expression.AndAlso(MakeValidation(lhs), MakeExpression(lhs));
                    else
                        exprs.Enqueue(MakeValidation(lhs));
                }
                if (MayNullable(rhs))
                {
                    if (Nullable.GetUnderlyingType(GetValueType(rhs)) == typeof(bool))
                        rhs = Expression.AndAlso(MakeValidation(rhs), MakeExpression(rhs));
                    else
                        exprs.Enqueue(MakeValidation(rhs));
                }
                exprs.Enqueue(op(MakeExpression(lhs), MakeExpression(rhs)));
                var e = exprs.Dequeue();
                while (exprs.Count != 0)
                {
                    e = Expression.AndAlso(e, exprs.Dequeue());
                }
                return e;
            }
            return op(MakeExpression(lhs), MakeExpression(rhs));
        }

        private Expression MakeInsensitiveBinaryExpression(BinaryExpressionBuilder op, object lhs, object rhs)
        {
            object[] leftArray = lhs as object[], rightArray = rhs as object[];
            if (leftArray != null && rightArray == null)
            {
                var exprs = new Queue<Expression>(leftArray.Select(o => MakeInsensitiveBinaryExpression(op, o, rhs)));
                var e = exprs.Dequeue();
                while (exprs.Count != 0)
                {
                    e = Expression.OrElse(e, exprs.Dequeue());
                }
                return e;
            }
            if (leftArray == null && rightArray != null)
            {
                var exprs = new Queue<Expression>(rightArray.Select(o => MakeInsensitiveBinaryExpression(op, lhs, o)));
                var e = exprs.Dequeue();
                while (exprs.Count != 0)
                {
                    e = Expression.OrElse(e, exprs.Dequeue());
                }
                return e;
            }
            if (leftArray != null) // always true -> && rightArray != null
            {
                var exprs = new Queue<Expression>(leftArray.SelectMany(l => rightArray.Select(r => MakeInsensitiveBinaryExpression(op, l, r))));
                var e = exprs.Dequeue();
                while (exprs.Count != 0)
                {
                    e = Expression.OrElse(e, exprs.Dequeue());
                }
                return e;
            }
            if (MayNullable(lhs) || MayNullable(rhs))
            {
                // left or right value may be null
                var exprs = new Queue<Expression>();
                if (MayNullable(lhs)) exprs.Enqueue(MakeValidation(lhs));
                if (MayNullable(rhs)) exprs.Enqueue(MakeValidation(rhs));
                exprs.Enqueue(op(MakeToLowerCase(lhs), MakeToLowerCase(rhs)));
                var e = exprs.Dequeue();
                while (exprs.Count != 0)
                {
                    e = Expression.AndAlso(e, exprs.Dequeue());
                }
                return e;
            }
            return op(MakeToLowerCase(lhs), MakeToLowerCase(rhs));
        }

        private Expression MakeLambdaExpression(Type type, CompilerToken arg, ASTNode body)
        {
            var p = Expression.Parameter(type, arg.Value);
            var tfn = typeof(Func<,>).MakeGenericType(type, typeof(bool));
            _env.Add(arg.Value, p);
            var one = MakeWrappedExpression(CompileOne(body));
            _env.Clear();
            return Expression.Lambda(tfn, one, body.Dump(), new[] { p });
        }

        private Expression MakeValidation(object o)
        {
            if (!MayNullable(o)) throw new Exception();
            var q = new Queue<Expression>();
            // Check all nullable properties
            var tmpSymbol = new CompilerToken { Type = TokenType.Symbol, Value = null };
            var props = ((CompilerToken)o).Value.Split('.');
            foreach (var prop in props)
            {
                if (!String.IsNullOrEmpty(tmpSymbol.Value)) tmpSymbol.Value += ".";
                tmpSymbol.Value += prop;
                if (!MayNullable(tmpSymbol, false)) continue;
                if (IsNullable(tmpSymbol))
                {
                    var t = new CompilerToken { Type = TokenType.Symbol, Value = (tmpSymbol).Value + ".HasValue" };
                    var lhs = MakePropertyAccess(t);
                    var rhs = Expression.Constant(true);
                    q.Enqueue(Expression.Equal(lhs, rhs));
                }
                else
                {
                    var lhs = MakePropertyAccess(tmpSymbol);
                    var rhs = Expression.Constant(null);
                    q.Enqueue(Expression.NotEqual(lhs, rhs));
                }
            }
            // Concat all expressions
            var e = q.Dequeue();
            while (q.Count > 0)
            {
                e = Expression.AndAlso(e, q.Dequeue());
            }
            return e;
        }

        private Expression MakeToLowerCase(object o)
        {
            if (o is Expression)
            {
                throw new ParseException("case insensitive option can only use to Property or String.", -1);
            }
            if (o is CompilerToken)
            {
                var t = (CompilerToken)o;
                var method = typeof(string).GetRuntimeMethod("ToLowerInvariant", new Type[0]);
                if (t.Type == TokenType.Symbol)
                {
                    var type = GetSymbolType(t);
                    if (type != typeof(string))
                    {
                        throw new ParseException("case insensitive option can only use to Property or String.", -1);
                    }
                    return Expression.Call(MakeExpression(o), method);
                }
                if (t.Type == TokenType.String)
                {
                    return Expression.Call(MakeExpression(o), method);
                }
            }
            throw new ParseException("case insensitive option can only use to Property or String.", -1);
        }

        private Expression MakeMemberAccessExpression(Expression lhs, CompilerToken rhs)
        {
            var valueExpr = MakeNullableAccessExpression(lhs);
            var leftType = valueExpr.Type;
            if(!rhs.IsSymbol()) throw new ParseException("Property accessor rhs should be Symbol.", rhs.Position);
            var prop = GetPreferredPropertyType(leftType, rhs.Value);
            return Expression.Property(valueExpr, prop);
        }

        private Expression MakeNullableAccessExpression(Expression expr)
        {
            var type = Nullable.GetUnderlyingType(expr.Type);
            if (type == null) return expr;
            var prop = expr.Type.GetRuntimeProperty("Value");
            return Expression.Property(expr, prop);
        }

        private Expression MakeWrappedExpression(object t)
        {
            if (t is Expression) return (Expression)t;
            if (MayNullable(t)) t = MakeValidation(t);
            return MakeExpression(t);
        }

        private bool MayNullable(object o, bool checkRecursive = true)
        {
            if (o is Expression) return false;
            if (o is CompilerToken)
            {
                var t = (CompilerToken)o;
                switch (t.Type)
                {
                    case TokenType.Symbol:
                        if (checkRecursive)
                        {
                            var tmpSymbol = new CompilerToken { Type = TokenType.Symbol, Value = null };
                            var props = t.Value.Split('.');
                            if (props[0] == UserFunctionNamespace) return false;
                            foreach (var prop in props)
                            {
                                if (!String.IsNullOrEmpty(tmpSymbol.Value)) tmpSymbol.Value += ".";
                                tmpSymbol.Value += prop;
                                var type = GetSymbolType(tmpSymbol);
                                if (!type.GetTypeInfo().IsValueType) return true;
                                if (Nullable.GetUnderlyingType(type) != null) return true;
                            }
                        }
                        else
                        {
                            var type = GetSymbolType(t);
                            if (!type.GetTypeInfo().IsValueType) return true;
                            if (Nullable.GetUnderlyingType(type) != null) return true;
                        }
                        return false;
                    case TokenType.String:
                    case TokenType.Double:
                    case TokenType.Long:
                    case TokenType.ULong:
                    case TokenType.Bool:
                    case TokenType.Null:
                        return false;
                }
            }
            throw new ArgumentException("Can't determin Type.");
        }

        private bool IsNullable(object o)
        {
            if (o is Expression) return false;
            if (o is CompilerToken)
            {
                var t = (CompilerToken)o;
                switch (t.Type)
                {
                    case TokenType.Symbol:
                        var type = GetSymbolType((CompilerToken)o);
                        if (!type.GetTypeInfo().IsValueType) return false;
                        if (Nullable.GetUnderlyingType(type) != null) return true;
                        return false;
                    case TokenType.String:
                    case TokenType.Double:
                    case TokenType.Long:
                    case TokenType.ULong:
                    case TokenType.Bool:
                    case TokenType.Null:
                        return false;
                }
            }
            throw new ArgumentException("Can't determin Type.");
        }

        private bool IsNullValue(object o)
        {
            if (o == null) return true;
            if (o is ConstantExpression) return ((ConstantExpression)o).Value == null;
            if (o is Expression) return false;
            if (o is CompilerToken) return ((CompilerToken)o).Type == TokenType.Null;
            return false;
        }

        private bool IsConstant(object o)
        {
            if (o is object[])
            {
                return ((object[])o).Select(IsConstant).Aggregate(false, (n, b) => n | b);
            }
            if (o is Expression) return false;
            if (o is CompilerToken)
            {
                var t = (CompilerToken)o;
                switch (t.Type)
                {
                    case TokenType.Symbol:
                    case TokenType.String:
                        return false;
                    case TokenType.Double:
                    case TokenType.Long:
                    case TokenType.ULong:
                    case TokenType.Bool:
                    case TokenType.Null:
                        return true;
                }
            }
            throw new ArgumentException("Can't determin Type.");
        }

        private object GetConstantValue(object o)
        {
            if (o is CompilerToken)
            {
                var t = (CompilerToken)o;
                switch (t.Type)
                {
                    case TokenType.Symbol:
                    case TokenType.String:
                        return null;
                    case TokenType.Double:
                        return Double.Parse(t.Value);
                    case TokenType.Long:
                        return Int64.Parse(t.Value);
                    case TokenType.ULong:
                        return UInt64.Parse(t.Value);
                    case TokenType.Bool:
                        return t.Value.ToLowerInvariant() == "true";
                    case TokenType.Null:
                        return null;
                }
            }
            return null;
        }

        private Type GetValueType(object o)
        {
            if (o is Expression) return ((Expression)o).Type;
            if (o is CompilerToken)
            {
                var t = (CompilerToken)o;
                switch (t.Type)
                {
                    case TokenType.Symbol:
                        return GetSymbolType(t);
                    case TokenType.String:
                        return typeof(string);
                    case TokenType.Double:
                        return typeof(double);
                    case TokenType.Long:
                        return typeof(long);
                    case TokenType.ULong:
                        return typeof(ulong);
                    case TokenType.Bool:
                        return typeof(bool);
                    case TokenType.Null:
                        return typeof(object);
                }
            }
            throw new ArgumentException("Can't convert to Expression.");
        }

        private Expression MakeExpression(object o)
        {
            if (o is Expression) return (Expression)o;
            if (o is CompilerToken)
            {
                var t = (CompilerToken)o;
                switch (t.Type)
                {
                    case TokenType.Symbol:
                        return MakePropertyAccess(t);
                    case TokenType.String:
                        return Expression.Constant(t.Value);
                    case TokenType.Double:
                    case TokenType.Long:
                    case TokenType.ULong:
                    case TokenType.Bool:
                    case TokenType.Null:
                        return Expression.Constant(GetConstantValue(o));
                }
            }
            throw new ArgumentException("Can't convert to Expression.");
        }

        private Expression MakeRegexExpression(Expression lhs, Expression rhs)
        {
            var ctor =
                Expression.New(
                    typeof(Regex).GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 2),
                    rhs, Expression.Constant(RegexOptions.Singleline));
            var regexObj = Expression.Parameter(typeof(Regex), "regex");
            var isMatchMethod = typeof(Regex).GetRuntimeMethod("IsMatch", new[] { typeof(string) });
            return Expression.Block(
                new[] { regexObj },
                Expression.Assign(regexObj, ctor),
                Expression.Call(regexObj, isMatchMethod, lhs));
        }

        private Expression MakeContainsExpression(Expression lhs, Expression rhs)
        {
            var containsMethod = typeof(string).GetRuntimeMethod("Contains", new[] { typeof(string) });
            return Expression.Call(lhs, containsMethod, rhs);
        }

        private Expression MakePropertyAccess(CompilerToken t)
        {
            if (t.Type != TokenType.Symbol) throw new ArgumentException("argument token is not symbol.");
            if (_env.ContainsKey(t.Value)) return _env[t.Value];
            var targetExpression = (Expression)_rootParameter;
            var targetType = typeof(T);
            var properties = (IsNullable(t) ? t.Value + ".Value" : t.Value).Split('.');
            foreach (var prop in properties)
            {
                var index = Int32.MaxValue;
                var propname = prop;
                if (targetType == typeof(UserFunctionTypePlaceholder))
                {
                    if (Functions.ContainsKey(propname.ToLowerInvariant()))
                    {
                        var func = Functions[propname.ToLowerInvariant()];
                        return Expression.Call(Expression.Constant(func),
                            func.GetType().GetRuntimeMethod("Invoke", new[] { typeof(T) }), _rootParameter);
                    }
                    throw new ParseException(String.Format("Property '{0}' is not found.", t.Value), -1);
                }
                if (propname == UserFunctionNamespace && properties.First() == prop)
                {
                    targetType = typeof(UserFunctionTypePlaceholder);
                    continue;
                }
                if (propname.EndsWith("]"))
                {
                    var arrayProp = Propname(propname);
                    propname = arrayProp.PropName;
                    index = arrayProp.Index;
                }
                var propInfo = GetPreferredPropertyType(targetType, propname);
                if (propInfo == null)
                    throw new ParseException(String.Format("Property '{0}' is not found.", t.Value), -1);
                targetExpression = Expression.MakeMemberAccess(targetExpression, propInfo);
                targetType = propInfo.PropertyType;
                if (index != Int32.MaxValue)
                {
                    if (targetType.IsArray)
                    {
                        targetExpression = Expression.ArrayIndex(targetExpression, Expression.Constant(index));
                        targetType = targetType.GetElementType();
                    }
                    else
                    {
                        propInfo = targetType.GetRuntimeProperty("Item");
                        if (propInfo == null) throw new ParseException(String.Format("Property '{0}' is not indexed type.", t.Value), -1);
                        var e = Expression.Constant(index);
                        targetExpression = Expression.MakeIndex(targetExpression, propInfo, new[] { e });
                        targetType = propInfo.PropertyType;
                    }
                }
            }
            return targetExpression;
        }

        private Type GetSymbolType(CompilerToken t)
        {
            if (t.Type != TokenType.Symbol) return null;
            if (_env.ContainsKey(t.Value)) return _env[t.Value].Type;
            var properties = t.Value.Split('.');
            var statusType = typeof(T);
            var targetType = statusType;
            foreach (var prop in properties)
            {
                var index = Int32.MaxValue;
                var propname = prop;
                if (targetType == typeof(UserFunctionTypePlaceholder))
                {
                    if (Functions.ContainsKey(prop.ToLowerInvariant())) return typeof(bool);
                    throw new ParseException(String.Format("Property '{0}' is not found.", t.Value), -1);
                }
                if (propname == UserFunctionNamespace && properties.First() == prop)
                {
                    targetType = typeof(UserFunctionTypePlaceholder);
                    continue;
                }
                if (propname.EndsWith("]"))
                {
                    var arrayProp = Propname(propname);
                    propname = arrayProp.PropName;
                    index = arrayProp.Index;
                }
                var propInfo = GetPreferredPropertyType(targetType, propname);
                if (propInfo == null)
                    throw new ParseException(String.Format("Property '{0}' is not found.", t.Value), -1);
                targetType = propInfo.PropertyType;
                if (index != Int32.MaxValue)
                {
                    if (targetType.IsArray) targetType = targetType.GetElementType();
                    else
                    {
                        var tmpProp = targetType.GetRuntimeProperty("Item");
                        if (tmpProp == null) throw new ParseException(String.Format("Property '{0}' is not indexed type.", t.Value), -1);
                        targetType = tmpProp.PropertyType;
                    }
                }
            }
            return targetType;
        }

        private static PropertyInfo GetPreferredPropertyType(Type targetType, string propname)
        {
            return targetType.GetRuntimeProperties().FirstOrDefault(p => p.Name.ToLowerInvariant() == propname.ToLowerInvariant());
        }

        private static IEnumerable<SyntaxInfo> ParseSymbolType(CompilerToken t)
        {
            if (t.Type != TokenType.Symbol) yield break;
            var properties = t.Value.Split('.');
            var statusType = typeof(T);
            var targetType = statusType;
            var pos = t.Position;
            foreach (var prop in properties)
            {
                var index = Int32.MaxValue;
                var propname = prop;
                var arrayAccessor = "";
                if (targetType == typeof(UserFunctionTypePlaceholder))
                {
                    var type = Functions.ContainsKey(propname.ToLowerInvariant())
                        ? SyntaxType.Identifier
                        : SyntaxType.Error;
                    yield return new SyntaxInfo
                    {
                        Position = pos,
                        Length = propname.Length,
                        SubType = TokenType.Symbol,
                        Type = type,
                        Value = propname
                    };
                    targetType = typeof(bool);
                    pos += propname.Length;
                }
                else if (propname.ToLowerInvariant() == UserFunctionNamespace && properties.First() == prop)
                {
                    yield return new SyntaxInfo
                    {
                        Position = pos,
                        Length = propname.Length,
                        SubType = TokenType.Symbol,
                        Type = SyntaxType.Identifier,
                        Value = propname
                    };
                    targetType = typeof(UserFunctionTypePlaceholder);
                    pos += propname.Length;
                }
                else
                {
                    if (propname.EndsWith("]"))
                    {
                        var arrayProp = Propname(propname);
                        propname = arrayProp.PropName;
                        index = arrayProp.Index;
                        arrayAccessor = arrayProp.ArrayAccessor;
                    }
                    var propInfo = GetPreferredPropertyType(targetType, propname);
                    if (propInfo == null)
                    {
                        yield return
                            new SyntaxInfo
                            {
                                Position = pos,
                                Length = t.Length - (pos - t.Position),
                                SubType = TokenType.Symbol,
                                Type = SyntaxType.Error,
                                Value = t.Value.Substring(pos - t.Position)
                            };
                        yield break;
                    }
                    yield return
                        new SyntaxInfo
                        {
                            Position = pos,
                            Length = propname.Length,
                            SubType = TokenType.Symbol,
                            Type = SyntaxType.Identifier,
                            Value = propname
                        };
                    targetType = propInfo.PropertyType;
                    pos += propname.Length;
                    if (index != Int32.MaxValue)
                    {
                        if (targetType.IsArray) targetType = targetType.GetElementType();
                        else
                        {
                            var tmpProp = targetType.GetRuntimeProperty("Item");
                            if (tmpProp == null)
                            {
                                yield return
                                    new SyntaxInfo
                                    {
                                        Position = pos,
                                        Length = t.Length - (pos - t.Position),
                                        SubType = TokenType.Symbol,
                                        Type = SyntaxType.Error,
                                        Value = t.Value.Substring(pos)
                                    };
                                yield break;
                            }
                            targetType = tmpProp.PropertyType;
                        }
                        yield return new SyntaxInfo
                        {
                            Position = pos,
                            Length = arrayAccessor.Length,
                            SubType = TokenType.Symbol,
                            Type = SyntaxType.ArrayArgs,
                            Value = arrayAccessor
                        };
                        pos += arrayAccessor.Length;
                    }
                }
                if (properties.Last() != prop)
                {
                    yield return new SyntaxInfo
                    {
                        Position = pos,
                        Length = 1,
                        SubType = TokenType.Symbol,
                        Type = SyntaxType.Constant,
                        Value = "."
                    };
                }
                pos += 1;
            }
        }

        private static ArrayProperty Propname(string propname)
        {
            var ret = new ArrayProperty();
            var arrayAccessor = default(string);
            var index = 0;
            var p = propname.IndexOf("[", StringComparison.Ordinal);
            if (p >= 0)
            {
                var i = propname.Substring(p + 1, propname.Length - p - 2);
                arrayAccessor = propname.Substring(p, propname.Length - p);
                if (!Int32.TryParse(i, out index)) index = Int32.MaxValue;
                else propname = propname.Substring(0, p);
            }
            ret.PropName = propname;
            ret.ArrayAccessor = arrayAccessor;
            ret.Index = index;
            return ret;
        }

        public static IEnumerable<CompilerToken> Parse(string text)
        {
            var c = new Tokenizer(text);
            return c.Parse(true);
        }

        public static Func<T, bool> Compile(IEnumerable<CompilerToken> tokenList)
        {
            var c = new FilterCompiler<T>();
            var ast = ASTBuilder.Generate(tokenList);
            return c.CompileSyntax(ast);
        }

        public static IEnumerable<SyntaxInfo> SyntaxHighlight(IEnumerable<CompilerToken> tokenList)
        {
            foreach (var t in tokenList)
            {
                switch (t.Type)
                {
                    case TokenType.Equals:
                    case TokenType.EqualsI:
                    case TokenType.NotEquals:
                    case TokenType.NotEqualsI:
                    case TokenType.Regexp:
                    case TokenType.Contains:
                    case TokenType.ContainsI:
                    case TokenType.And:
                    case TokenType.Or:
                    case TokenType.Not:
                    case TokenType.LessThan:
                    case TokenType.GreaterThan:
                    case TokenType.LessThanEquals:
                    case TokenType.GreaterThanEquals:
                    case TokenType.OpenBracket:
                    case TokenType.CloseBracket:
                    case TokenType.ArrayStart:
                    case TokenType.ArrayEnd:
                    case TokenType.ArrayDelimiter:
                    case TokenType.Lambda:
                        yield return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Operator, Value = t.Value };
                        break;
                    case TokenType.Symbol:
                        foreach (var a in ParseSymbolType(t)) yield return a;
                        break;
                    case TokenType.String:
                        yield return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.String, Value = t.Value };
                        break;
                    case TokenType.Double:
                    case TokenType.Long:
                    case TokenType.ULong:
                        yield return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Constant, Value = t.Value };
                        break;
                    case TokenType.Bool:
                    case TokenType.Null:
                        yield return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Keyword, Value = t.Value };
                        break;
                }
            }
        }

        public static Func<T, bool> Compile(string text)
        {
            var c = new FilterCompiler<T>();
            var ast = c.Tokenize(text);
            return c.CompileSyntax(ast);
        }

        public static IEnumerable<SyntaxInfo> ParseForSyntaxHightlight(string text)
        {
            var c = new Tokenizer(text);
            return SyntaxHighlight(c.Parse(true));
        }

        public static IEnumerable<string> Completion(string text, out string prefix)
        {
            var f = new FilterCompiler<T>();
            var c = new Tokenizer(text);
            var syntax = SyntaxHighlight(c.Parse(true)).ToArray();
            var q = syntax.Reverse()
                    .TakeWhile(t => t.SubType == TokenType.Symbol)
                    .SkipWhile(t => string.IsNullOrWhiteSpace(t.Value))
                    .ToList();
            prefix = "";
            if (text.EndsWith("'") || text.EndsWith("\"")) return new List<string>();
            if (q.Count == 0)
            {
                var last = syntax.LastOrDefault();
                if (last != null && last.Type == SyntaxType.String) return new List<string>();
                // Symbolが含まれないときはStatusのプロパティ
                return typeof(T).GetRuntimeProperties()
                    .Select(p => p.Name)
                    .Concat(new[] { UserFunctionNamespace })
                    .Select(s => s.ToLowerInvariant())
                    .OrderBy(s => s);
            }
            // 最後が '.' ならプロパティを見る
            var lookup = q[0].Value == ".";
            q = q.Where(t => t.Value != ".").ToList();
            // 2個以上エラーは空
            if (q.Count(t => t.Type == SyntaxType.Error) > 1) return new List<string>();
            // 型を検索する
            // 先頭がエラーなので2個目以降をくっつける
            var tokenList = ConcatiateSyntax(q.Skip(lookup ? 0 : 1).Reverse().ToList());
            var symbol = string.Join(".", tokenList.Select(t => t.Value));
            // 絞り込み文字列
            prefix = lookup ? "" : q[0].Value;
            // 型情報を検索
            var targetType = string.IsNullOrEmpty(symbol) ? typeof(T) : f.GetSymbolType(new CompilerToken
            {
                Length = symbol.Length,
                Position = 0,
                Type = TokenType.Symbol,
                Value = symbol
            });
            // 型が見つからないときは空
            if (targetType == null) return new List<string>();
            // プロパティ一覧を返却
            var pp = prefix;
            return targetType.GetRuntimeProperties()
                .Select(p => p.Name)
                .Concat(targetType == typeof(T) && !string.IsNullOrEmpty(UserFunctionNamespace) ? new[] { UserFunctionNamespace } : Enumerable.Empty<string>())
                .Concat(targetType == typeof(UserFunctionTypePlaceholder) ? Functions.Keys : Enumerable.Empty<string>())
                .Select(s => s.ToLowerInvariant())
                .Where(n => n.StartsWith(pp))
                .OrderBy(s => s);
        }

        private static IEnumerable<SyntaxInfo> ConcatiateSyntax(IEnumerable<SyntaxInfo> syntaxInfos)
        {
            var list = new List<SyntaxInfo>();
            foreach (var info in syntaxInfos)
            {
                if (info.Type == SyntaxType.ArrayArgs)
                {
                    list.Last().Length += info.Length;
                    list.Last().Value += info.Value;
                }
                else
                {
                    list.Add(info);
                }
            }
            return list;
        }
    }
}
