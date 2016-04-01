using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Freesia.Internal;
using Freesia.Internal.Extensions;
using Freesia.Internal.Reflection;
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
            if (ast.Token.Type == TokenType.IndexerNode)
            {
                return MakeIndexerExpression(ast.Left, ast.Right);
            }
            if (!ast.Token.IsOperator)
            {
                return ast.Token;
            }
            if (ast.Token.Type == TokenType.Lambda)
            {
                // Parse for Lambda
                return MakeLambdaExpression(typeof(object), ast.Left.Token, ast.Right);
            }
            if (ast.Token.Type == TokenType.InvokeMethod)
            {
                // Parse for MethodInvoke
                return MakeMethodInvokeExpression(ast.Left, ast.Right);
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
                    return MakeMemberAccessExpression(lhs, (CompilerToken)rhs);
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
            var props = new Queue<Expression>();
            while (o is MemberExpression)
            {
                var expr = o as MemberExpression;
                if(MayNullable(expr, false)) props.Enqueue(expr);
                o = expr.Expression;
            }
            if (o is CompilerToken)
            {
                if(MayNullable(o, false)) props.Enqueue(MakePropertyAccess((CompilerToken) o));
            }
            foreach (var prop in props)
            {
                if (IsNullable(prop))
                {
                    var t = new CompilerToken { Type = TokenType.Symbol, Value = "HasValue" };
                    var lhs = MakeMemberAccessExpression(prop, t);
                    var rhs = Expression.Constant(true);
                    q.Enqueue(Expression.Equal(lhs, rhs));
                }
                else
                {
                    var rhs = Expression.Constant(null);
                    q.Enqueue(Expression.NotEqual(prop, rhs));
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
            var expr = MakeExpression(o);
            if (expr.Type == typeof(string))
            {
                return Expression.Call(expr, Cache.StringToLowerInvariant.Value);
            }
            throw new ParseException("case insensitive option can only use to Property or String.", -1);
        }

        private Expression MakeIndexerExpression(ASTNode prop, ASTNode indexer)
        {
            var property = MakeExpression(prop.Token);
            var i = GetConstantValue(indexer.Token);
            if (i is Double) throw new ParseException("Indexer should be int value.", indexer.Token.Position);
            if (property.Type.IsArray)
            {
                return Expression.ArrayIndex(property, Expression.Constant(Convert.ToInt32(i)));
            }
            var propInfo = property.Type.GetRuntimeProperty("Item");
            if (propInfo == null)
                throw new ParseException(String.Format("Property '{0}' is not indexed type.", prop.Token.Value), -1);
            var e = Expression.Constant(Convert.ToInt32(i));
            return Expression.MakeIndex(property, propInfo, new[] { e });
        }

        private Expression MakeMemberAccessExpression(object lhs, CompilerToken rhs)
        {
            var expr = lhs as Expression ?? MakePropertyAccess((CompilerToken)lhs);
            var valueExpr = MakeNullableAccessExpression(expr);
            if (IsNullable(lhs) && rhs.Value.ToLowerInvariant() == "hasvalue") valueExpr = expr;
            var leftType = valueExpr.Type;
            if (!rhs.IsSymbol) throw new ParseException("Property accessor rhs should be Symbol.", rhs.Position);
            if (valueExpr.Type == typeof(UserFunctionTypePlaceholder))
            {
                if (!Functions.ContainsKey(rhs.Value.ToLowerInvariant()))
                    throw new ParseException(String.Format("Property '{0}' is not found.", rhs.Value), -1);
                var func = Functions[rhs.Value.ToLowerInvariant()];
                return Expression.Call(Expression.Constant(func),
                    func.GetType().GetRuntimeMethod("Invoke", new[] { typeof(T) }), _rootParameter);
            }
            var prop = GetPreferredPropertyType(leftType, rhs.Value);
            return Expression.Property(valueExpr, prop);
        }

        private Expression MakeMethodInvokeExpression(ASTNode lhs, ASTNode rhs)
        {
            if (lhs.Token.Type != TokenType.PropertyAccess)
                throw new ParseException("Invoke method requires property access.", lhs.Token.Position);
            if (rhs.Token.Type != TokenType.Lambda)
                throw new ParseException("Invoke method requires lambda expression.", rhs.Token.Position);
            var rootExpr = MakeExpression(CompileOne(lhs.Left));
            var ie = rootExpr.Type.GetUnderlyingEnumerableType();
            if (ie == null)
                throw new ParseException("Method only apply to IE<T>.", lhs.Token.Position);
            var argType = ie.GenericTypeArguments[0]; // IE< 'T' >
            // build lambda expression
            var closure = MakeLambdaExpression(argType, rhs.Left.Token, rhs.Right);
            // apply method
            var method = default(MethodInfo);
            var methodName = lhs.Right.Token.Value.ToLowerInvariant();
            var closureExpr = ((LambdaExpression)closure).Compile();
            switch (methodName)
            {
                case "contains":
                    method = ExtensionMethods.Methods[methodName](new[] { argType });
                    break;
                default:
                    throw new ParseException($"Method {lhs.Right.Token.Value} is not supported.", lhs.Right.Token.Position);
            }
            return Expression.Call(method, rootExpr, Expression.Constant(closureExpr));
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
            if (o is MemberExpression)
            {
                while (o is MemberExpression)
                {
                    var expr = o as MemberExpression;
                    if (!expr.Type.GetTypeInfo().IsValueType) return true;
                    if (Nullable.GetUnderlyingType(expr.Type) != null) return true;
                    o = expr.Expression;
                    if (!checkRecursive) return false;
                }
            }
            if (o is Expression) return false;
            if (o is CompilerToken)
            {
                var t = (CompilerToken)o;
                switch (t.Type)
                {
                    case TokenType.Symbol:
                        var type = GetSymbolType(t);
                        if (!type.GetTypeInfo().IsValueType) return true;
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

        private bool IsNullable(object o)
        {
            if (o is MemberExpression)
            {
                var expr = o as MemberExpression;
                if (!expr.Type.GetTypeInfo().IsValueType) return false;
                if (Nullable.GetUnderlyingType(expr.Type) != null) return true;
                return false;
            }
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
                        return MakeNullableAccessExpression(MakePropertyAccess(t));
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
            return Expression.Block(
                new[] { regexObj },
                Expression.Assign(regexObj, ctor),
                Expression.Call(regexObj, Cache.RegexIsMatch.Value, lhs));
        }

        private Expression MakeContainsExpression(Expression lhs, Expression rhs)
        {
            return Expression.Call(lhs, Cache.StringContains.Value, rhs);
        }

        private Expression MakePropertyAccess(CompilerToken t)
        {
            if (t.Type != TokenType.Symbol) throw new ArgumentException("argument token is not symbol.");
            if (_env.ContainsKey(t.Value)) return _env[t.Value];
            var targetExpression = (Expression)_rootParameter;
            var targetType = typeof(T);
            var propname = t.Value;
            if (propname == UserFunctionNamespace)
            {
                return Expression.Constant(null, typeof(UserFunctionTypePlaceholder));
            }
            var propInfo = GetPreferredPropertyType(targetType, propname);
            if (propInfo == null)
                throw new ParseException(String.Format("Property '{0}' is not found.", t.Value), -1);
            targetExpression = Expression.MakeMemberAccess(targetExpression, propInfo);
            return targetExpression;
        }

        private Type GetSymbolType(CompilerToken t)
        {
            if (t.Type != TokenType.Symbol) return null;
            if (_env.ContainsKey(t.Value)) return _env[t.Value].Type;
            var propname = t.Value;
            if (propname == UserFunctionNamespace)
            {
                return typeof(UserFunctionTypePlaceholder);
            }
            var propInfo = GetPreferredPropertyType(typeof(T), propname);
            if (propInfo == null)
                throw new ParseException(String.Format("Property '{0}' is not found.", t.Value), -1);
            return propInfo.PropertyType;
        }

        private static PropertyInfo GetPreferredPropertyType(Type targetType, string propname)
        {
            return targetType?.GetRuntimeProperties().FirstOrDefault(p => p.Name.ToLowerInvariant() == propname.ToLowerInvariant());
        }

        private static IEnumerable<SyntaxInfo> ParseSymbolType(Queue<CompilerToken> symbols, Type argType, string argName)
        {
            var targetType = typeof(T);
            var indexer = 0;
            // yield break for no symbols
            if (symbols.Count == 0) yield break;
            // lambda argument
            if (symbols.First().Value == argName)
            {
                targetType = argType;
                yield return new SyntaxInfo(symbols.Dequeue(), SyntaxType.Argument) { TypeInfo = targetType };
            }
            foreach (var prop in symbols)
            {
                var propname = prop.Value;
                var syntaxType = default(SyntaxType);
                if (prop.Type == TokenType.PropertyAccess)
                {
                    yield return new SyntaxInfo(prop, SyntaxType.Operator) { Value = "." };
                    continue;
                }
                if (prop.Type == TokenType.IndexerStart)
                {
                    indexer++;
                    yield return TranslateSyntaxInfo(prop);
                    continue;
                }
                if (prop.Type == TokenType.IndexerEnd)
                {
                    indexer--;
                    var s = TranslateSyntaxInfo(prop);
                    if (indexer == 0)
                    {
                        s.TypeInfo = targetType.GetElementType();
                        targetType = s.TypeInfo;
                    }
                    yield return s;
                    continue;
                }
                if (indexer > 0)
                {
                    yield return TranslateSyntaxInfo(prop);
                    continue;
                }
                if (propname.ToLowerInvariant() == UserFunctionNamespace && targetType == typeof(T))
                {
                    syntaxType = SyntaxType.Identifier;
                    targetType = typeof(UserFunctionTypePlaceholder);
                }
                else if (targetType == typeof(UserFunctionTypePlaceholder))
                {
                    syntaxType = Functions.ContainsKey(prop.Value) ? SyntaxType.Identifier : SyntaxType.Error;
                    targetType = null;
                }
                else
                {
                    var propInfo = GetPreferredPropertyType(targetType, propname);
                    syntaxType = propInfo == null ? SyntaxType.Error : SyntaxType.Identifier;
                    if (syntaxType == SyntaxType.Error && (targetType?.IsEnumerable() ?? false))
                    {
                        syntaxType = ExtensionMethods.Methods.ContainsKey(prop.Value.ToLowerInvariant())
                            ? SyntaxType.Identifier
                            : SyntaxType.Error;
                        targetType = null;
                    }
                    else
                    {
                        targetType = propInfo?.PropertyType;
                    }
                }
                yield return new SyntaxInfo(prop, syntaxType) { TypeInfo = targetType };
                if (syntaxType == SyntaxType.Error) yield break;
            }
        }

        private static IEnumerable<CompilerToken> TakeSymbols(IEnumerator<CompilerToken> list)
        {
            var indexer = 0;
            while (list.MoveNext())
            {
                var a = list.Current;
                if (a.Type == TokenType.IndexerStart) { indexer++; yield return a; }
                else if (a.Type == TokenType.IndexerEnd) { indexer--; yield return a; }
                else if (a.Type == TokenType.Symbol) yield return a;
                else if (a.Type == TokenType.PropertyAccess) yield return a;
                else if (indexer > 0) yield return a;
                else yield break;
            }
        }

        private static IEnumerable<SyntaxInfo> TakeSymbolsForCompletion(IEnumerable<SyntaxInfo> list)
        {
            var indexer = 0;
            foreach (var a in list)
            {
                if (a.SubType == TokenType.IndexerStart) { indexer++; yield return a; }
                else if (a.SubType == TokenType.IndexerEnd) { indexer--; yield return a; }
                else if (a.SubType == TokenType.Symbol) yield return a;
                else if (a.SubType == TokenType.PropertyAccess) yield return a;
                else if (indexer > 0) yield return a;
                else yield break;
            }
        }

        private static SyntaxInfo TranslateSyntaxInfo(CompilerToken t)
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
                case TokenType.IndexerStart:
                case TokenType.IndexerEnd:
                case TokenType.Lambda:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Operator, Value = t.Value };
                case TokenType.String:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.String, Value = t.Value };
                case TokenType.Double:
                case TokenType.Long:
                case TokenType.ULong:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Constant, Value = t.Value };
                case TokenType.Bool:
                case TokenType.Null:
                    return new SyntaxInfo { Length = t.Length, Position = t.Position, SubType = t.Type, Type = SyntaxType.Keyword, Value = t.Value };
                default:
                    throw new ParseException("#-1", t.Position);
            }
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
            var pendingSymbols = new Queue<CompilerToken>();
            var lambdaParsing = false;
            var brackets = 0;
            var argstack = new Stack<Tuple<string, Type, int>>();
            var argname = default(string);
            var argtype = default(Type);
            var latestResolvedType = default(Type);
            var enumerator = tokenList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var t = enumerator.Current;
                // read symbols
                if (t.Type == TokenType.Symbol || t.Type == TokenType.PropertyAccess)
                {
                    pendingSymbols.Enqueue(t);
                    foreach (var s in TakeSymbols(enumerator)) pendingSymbols.Enqueue(s);
                    t = enumerator.Current;
                }
                // enter Lambda parsing mode
                if (t.Type == TokenType.Lambda)
                {
                    if (lambdaParsing)
                    {
                        argstack.Push(Tuple.Create(argname, argtype, brackets));
                    }
                    lambdaParsing = true;
                    brackets = 1;
                    var arg = pendingSymbols.Dequeue();
                    yield return new SyntaxInfo(arg, SyntaxType.Argument);
                    yield return new SyntaxInfo(t, SyntaxType.Operator);
                    argname = arg.Value;
                    argtype = latestResolvedType?.GetElementType();
                    pendingSymbols.Clear();
                    continue;
                }
                if (lambdaParsing)
                {
                    if (t.Type == TokenType.OpenBracket) { brackets++; }
                    if (t.Type == TokenType.CloseBracket) { brackets--; }
                }
                if (pendingSymbols.Count > 0)
                {
                    foreach (var a in ParseSymbolType(pendingSymbols, argtype, argname))
                    {
                        if (a.TypeInfo != null) latestResolvedType = a.TypeInfo;
                        yield return a;
                    }
                    pendingSymbols.Clear();
                }
                if (lambdaParsing && brackets == 0)
                {
                    lambdaParsing = false;
                    argname = null;
                    // restore previews environment
                    if (argstack.Count > 0)
                    {
                        var prev = argstack.Pop();
                        argname = prev.Item1;
                        argtype = prev.Item2;
                        brackets = prev.Item3;
                        lambdaParsing = true;
                    }
                }
                // process rest of token
                if (t.Type != TokenType.Symbol && t.Type != TokenType.PropertyAccess)
                {
                    yield return TranslateSyntaxInfo(t);
                }
            }
            // 残ってたら全部出す
            foreach (var a in ParseSymbolType(pendingSymbols, argtype, argname)) yield return a;
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
            var c = new Tokenizer(text);
            var syntax = SyntaxHighlight(c.Parse(true)).ToArray();
            var q = TakeSymbolsForCompletion(syntax.Reverse()).ToList();
            prefix = "";
            if (text.EndsWith("'") || text.EndsWith("\"")) return new List<string>();
            var last = q.FirstOrDefault();
            // 末尾が文字列なら空
            if (syntax.LastOrDefault()?.Type == SyntaxType.String) return new List<string>();
            // 末尾がnullならtypeof(T)のプロパティ
            if (last == null) return typeof(T).GetRuntimeProperties()
                 .Select(p => p.Name)
                 .Concat(string.IsNullOrEmpty(UserFunctionNamespace) ? Enumerable.Empty<string>() : new[] { UserFunctionNamespace })
                 .Select(s => s.ToLowerInvariant())
                 .OrderBy(s => s);
            // 末尾が '[', ']' なら空
            if (last.SubType == TokenType.IndexerStart) return new List<string>();
            if (last.SubType == TokenType.IndexerEnd) return new List<string>();
            // 最後が '.' ならプロパティを見る
            var lookup = last.SubType == TokenType.PropertyAccess;
            var type = default(Type);
            if (last.SubType == TokenType.PropertyAccess)
            {
                type = q.Skip(1).FirstOrDefault()?.TypeInfo;
            }
            // 末尾がエラーなら直前の要素
            if (last.Type == SyntaxType.Error)
            {
                type = q.Count > 2 ? q.Skip(2).FirstOrDefault()?.TypeInfo : typeof(T);
            }
            // 2個以上エラーは空
            if (q.Count(t => t.Type == SyntaxType.Error) > 1) return new List<string>();
            if (type == null) return new List<string>();
            // 絞り込み文字列
            var pp = prefix = lookup ? "" : q[0].Value;
            // プロパティ一覧を返却
            return type.GetRuntimeProperties()
                .Select(p => p.Name)
                .Concat(type == typeof(T) && !string.IsNullOrEmpty(UserFunctionNamespace) ? new[] { UserFunctionNamespace } : Enumerable.Empty<string>())
                .Concat(type == typeof(UserFunctionTypePlaceholder) ? Functions.Keys : Enumerable.Empty<string>())
                .Concat(type.IsEnumerable() ? ExtensionMethods.Methods.Keys : Enumerable.Empty<string>())
                .Select(s => s.ToLowerInvariant())
                .Where(n => n.StartsWith(pp))
                .OrderBy(s => s);
        }
    }
}
