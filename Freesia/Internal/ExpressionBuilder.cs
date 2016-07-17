using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Freesia.Internal.Extensions;
using Freesia.Internal.Reflection;
using Freesia.Internal.Types;
using Freesia.Types;

namespace Freesia.Internal
{
    internal class ExpressionBuilder<T> : CompilerConfig<T>
    {
        private readonly ParameterExpression _rootParameter = Expression.Parameter(typeof(T), "status");
        private readonly Dictionary<string, ParameterExpression> _env = new Dictionary<string, ParameterExpression>();

        private delegate Expression BinaryExpressionBuilder(Expression lhs, Expression rhs);

        private delegate Expression UnaryExpressionBuilder(Expression expr);

        private object CompileOne(ASTNode ast)
        {
            if (ast == null) return null;
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
                return MakeIndexerExpression(ast.Left, ast.Right);
            if (!ast.Token.IsOperator)
                return ast.Token;
            switch (ast.Token.Type)
            {
                case TokenType.Lambda:
                    return MakeLambdaExpression(typeof(object), ast.Left.Token, ast.Right);
                case TokenType.InvokeMethod:
                    return MakeMethodInvokeExpression(ast.Left, ast.Right);
                case TokenType.Not:
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
                if (iscl && iscr) ; // 両方Constとか知らない
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

        public Func<T, bool> CompileSyntax(ASTNode ast)
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
            _env.Add(arg.Value, p);
            var one = MakeExpression(CompileOne(body));
            _env.Clear();
            var tfn = typeof(Func<,>).MakeGenericType(type, one.Type);
            return Expression.Lambda(tfn, one, body.Dump(), new[] { p });
        }

        private Expression MakeValidation(object o)
        {
            if (!MayNullable(o)) throw new Exception();
            var q = new Stack<Expression>();
            // Check all nullable properties
            var props = new Queue<Expression>();
            while (o is MemberExpression)
            {
                var expr = o as MemberExpression;
                if (MayNullable(expr, false)) props.Enqueue(expr);
                o = expr.Expression;
            }
            if (o is CompilerToken)
                if (MayNullable(o, false)) props.Enqueue(MakePropertyAccess((CompilerToken)o));
            foreach (var prop in props)
            {
                if (IsNullable(prop))
                {
                    var t = new CompilerToken { Type = TokenType.Symbol, Value = "HasValue" };
                    var lhs = MakeMemberAccessExpression(prop, t);
                    var rhs = Expression.Constant(true);
                    q.Push(Expression.Equal(lhs, rhs));
                }
                else
                {
                    var rhs = Expression.Constant(null);
                    q.Push(Expression.NotEqual(prop, rhs));
                }
            }
            // Concat all expressions
            var e = q.Pop();
            while (q.Count > 0)
            {
                e = Expression.AndAlso(e, q.Pop());
            }
            return e;
        }

        private Expression MakeToLowerCase(object o)
        {
            var expr = MakeExpression(o);
            if (expr.Type == typeof(string))
                return Expression.Call(expr, Cache.StringToLowerInvariant.Value);
            throw new ParseException("case insensitive option can only use to Property or String.", -1);
        }

        private Expression MakeIndexerExpression(ASTNode prop, ASTNode indexer)
        {
            var property = MakeExpression(prop.Token);
            var i = GetConstantValue(indexer.Token);
            if (i is Double) throw new ParseException("Indexer should be int value.", indexer.Token.Position);
            if (property.Type.IsArray)
                return Expression.ArrayIndex(property, Expression.Constant(Convert.ToInt32(i)));
            var propInfo = property.Type.GetRuntimeProperty("Item");
            if (propInfo == null)
                throw new ParseException($"Property '{prop.Token.Value}' is not indexed type.", -1);
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
                    throw new ParseException($"Property '{rhs.Value}' is not found.", -1);
                var func = Functions[rhs.Value.ToLowerInvariant()];
                return Expression.Call(Expression.Constant(func),
                    func.GetType().GetRuntimeMethod("Invoke", new[] { typeof(T) }), _rootParameter);
            }
            var prop = leftType.GetPreferredPropertyType(rhs.Value);
            return Expression.Property(valueExpr, prop);
        }

        private Expression MakeMethodInvokeExpression(ASTNode lhs, ASTNode rhs)
        {
            if (lhs.Token.Type != TokenType.PropertyAccess)
                throw new ParseException("Invoke method requires property access.", lhs.Token.Position);
            var rootExpr = MakeExpression(CompileOne(lhs.Left));
            var ie = rootExpr.Type.GetUnderlyingEnumerableType();
            if (ie == null)
                throw new ParseException("Method only apply to IE<T>.", lhs.Token.Position);
            // build argument list
            var args = new[] { rootExpr }.Concat(MakeArgumentList(ie.GetUnderlyingElementType(), rhs)).ToArray();
            var argTypes = args.Select(x => x.Type).ToArray();
            argTypes[0] = ie;
            // apply method
            var methodName = lhs.Right.Token.Value.ToLowerInvariant();
            var info = Helper.FindPreferredMethod(methodName, argTypes);
            if (info == null)
                throw new ParseException($"Could not found preferred method '{methodName}'.", lhs.Right.Token.Position);
            var callExpr = Expression.Call(info, args);
            // TODO: Null check
            //return MayNullable(rootExpr) ? Expression.(MakeValidation(rootExpr), callExpr) : (Expression)callExpr;
            return callExpr;
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
            var ctor = Expression.New(
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
                return Expression.Constant(null, typeof(UserFunctionTypePlaceholder));
            var propInfo = targetType.GetPreferredPropertyType(propname);
            if (propInfo == null)
                throw new ParseException($"Property '{t.Value}' is not found.", -1);
            targetExpression = Expression.MakeMemberAccess(targetExpression, propInfo);
            return targetExpression;
        }

        private Expression[] MakeArgumentList(Type elementType, ASTNode node)
        {
            if (node.Token.Type == TokenType.Nop) return new Expression[0];
            var stack = new Stack<ASTNode>();
            stack.Push(node);
            while (stack.Count > 0)
            {
                var n = stack.Pop();
                if (n.Token.Type != TokenType.ArrayDelimiter)
                {
                    stack.Push(n);
                    break;
                }
                stack.Push(n.Right);
                stack.Push(n.Left);
            }
            return stack.Select(x => x.Token.Type == TokenType.Lambda
                ? MakeLambdaExpression(elementType, x.Left.Token, x.Right)
                : MakeExpression(CompileOne(x))).ToArray();
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

        private Type GetSymbolType(CompilerToken t)
        {
            if (t.Type != TokenType.Symbol) return null;
            if (_env.ContainsKey(t.Value)) return _env[t.Value].Type;
            var propname = t.Value;
            if (propname == UserFunctionNamespace)
                return typeof(UserFunctionTypePlaceholder);
            var propInfo = typeof(T).GetPreferredPropertyType(propname);
            if (propInfo == null)
                throw new ParseException($"Property '{t.Value}' is not found.", -1);
            return propInfo.PropertyType;
        }

    }
}
