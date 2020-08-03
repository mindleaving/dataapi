using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Commons.Extensions;
using DataAPI.Client.Repositories;
using Newtonsoft.Json;

namespace DataAPI.Client
{
    internal static class ExpressionParser
    {
        public static string ParseQueryExpression(Expression expression)
        {
            var queryParts = new Stack<string>();
            ParseQueryExpressionImpl(expression, queryParts);
            var query = string.Join(" ", queryParts);
            return query;
        }

        private static void ParseQueryExpressionImpl(Expression expression, Stack<string> stack)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Call:
                    var callExpression = (MethodCallExpression) expression;
                    var isExtensionMethod = callExpression.Object == null;
                    var inputObject = isExtensionMethod ? callExpression.Arguments.First() : callExpression.Object;
                    var arguments = isExtensionMethod ? callExpression.Arguments.Skip(1) : callExpression.Arguments;
                    switch (callExpression.Method.Name)
                    {
                        case nameof(Queryable.Select):
                            stack.Push("SELECT " + string.Join(", ", ParseSelectArguments(arguments)));
                            break;
                        case nameof(Queryable.Where):
                            stack.Push("WHERE " + string.Join(", ", ParseWhereExpression(arguments.Single())));
                            break;
                        case nameof(Queryable.OrderBy):
                            stack.Push("ORDER BY " + ExtractPath(arguments.Single()) + " ASC");
                            break;
                        case nameof(Queryable.OrderByDescending):
                            stack.Push("ORDER BY " + ExtractPath(arguments.Single()) + " DESC");
                            break;
                        case nameof(Queryable.ThenBy):
                            stack.Push(", " + ExtractPath(arguments.Single()) + " ASC");
                            break;
                        case nameof(Queryable.ThenByDescending):
                            stack.Push(", " + ExtractPath(arguments.Single()) + " DESC");
                            break;
                        case nameof(Queryable.Take):
                            stack.Push("LIMIT " + ((ConstantExpression) arguments.Single()).Value);
                            break;
                        case nameof(Queryable.First):
                        case nameof(Queryable.FirstOrDefault):
                            stack.Push("LIMIT 1");
                            stack.Push("WHERE " + string.Join(", ", ParseWhereExpression(arguments.Single())));
                            break;
                        case nameof(Queryable.Single):
                        case nameof(Queryable.SingleOrDefault):
                            throw new NotSupportedException($"Use {nameof(Queryable.FirstOrDefault)} instead if you doesn't need the check for uniqueness.");
                        case nameof(Queryable.Skip):
                            stack.Push("SKIP " + ((ConstantExpression) arguments.Single()).Value);
                            break;
                    }
                    ParseQueryExpressionImpl(inputObject, stack);
                    break;
                case ExpressionType.Constant:
                    var constantExpression = (ConstantExpression) expression;
                    if (constantExpression.Value is GenericDatabase repository)
                    {
                        stack.Push($"FROM {repository.CollectionName}");
                    }
                    if (constantExpression.Value is IQueryable queryable)
                    {
                        stack.Push($"FROM {CollectionNameDeterminer.GetCollectionName(queryable.ElementType)}");
                    }
                    else
                        throw new NotSupportedException();
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private static IEnumerable<string> ParseSelectArguments(IEnumerable<Expression> selectArgumentExpressions)
        {
            foreach (var selectArgumentExpression in selectArgumentExpressions)
            {
                yield return ExtractPath(selectArgumentExpression);
            }
        }

        public static string ParseWhereExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return ExtractPath(expression);
                case ExpressionType.Quote:
                    return ParseWhereExpression(((UnaryExpression)expression).Operand);
                case ExpressionType.Lambda:
                    return ParseWhereExpression(((LambdaExpression) expression).Body);
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return ParseBinaryExpression((BinaryExpression)expression, "AND");
                case ExpressionType.Equal:
                    return ParseBinaryExpression((BinaryExpression)expression, "=", omitParentheses: true);
                case ExpressionType.ExclusiveOr:
                    throw new NotSupportedException("XOR is not supported");
                case ExpressionType.GreaterThan:
                    return ParseBinaryExpression((BinaryExpression)expression, ">", omitParentheses: true);
                case ExpressionType.GreaterThanOrEqual:
                    return ParseBinaryExpression((BinaryExpression)expression, ">=", omitParentheses: true);
                case ExpressionType.IsFalse:
                    return $"{ExtractPath(expression)} = false";
                case ExpressionType.Not:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    var unaryExpression = (UnaryExpression)expression;
                    if(unaryExpression.Operand is MemberExpression)
                        return $"{ExtractPath(unaryExpression.Operand)} = false";
                    throw new NotSupportedException("Negation of anything but fields is not supported");
                case ExpressionType.IsTrue:
                case ExpressionType.MemberAccess:
                    if(IsPathReference(expression))
                        return $"{ExtractPath(expression)} = true";
                    else
                        return GetValue(expression);
                case ExpressionType.Call:
                    if (IsContainsStatement(expression))
                        return BuildContainsExpression((MethodCallExpression)expression);
                    if (IsStringMatchStatement(expression))
                        return BuildStringMatchStatement((MethodCallExpression) expression);
                    return GetValue(expression);
                case ExpressionType.LessThan:
                    return ParseBinaryExpression((BinaryExpression)expression, "<", omitParentheses: true);
                case ExpressionType.LessThanOrEqual:
                    return ParseBinaryExpression((BinaryExpression)expression, "<=", omitParentheses: true);
                case ExpressionType.NotEqual:
                    return ParseBinaryExpression((BinaryExpression)expression, "!=", omitParentheses: true);
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return ParseBinaryExpression((BinaryExpression)expression, "OR");
                case ExpressionType.Constant:
                    return GetValue(expression);
                case ExpressionType.Conditional:
                    var conditionExpression = (ConditionalExpression) expression;
                    var isTrue = GetValue(conditionExpression.Test, omitQuotes: true).ToLowerInvariant() == "true";
                    if (isTrue)
                        return ParseWhereExpression(conditionExpression.IfTrue);
                    else
                        return ParseWhereExpression(conditionExpression.IfFalse);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool IsContainsStatement(Expression expression)
        {
            if (!(expression is MethodCallExpression methodCallExpression))
                return false;
            return methodCallExpression.Method.Name.InSet("Contains", "InSet");
        }

        private static bool IsStringMatchStatement(Expression expression)
        {
            if (!(expression is MethodCallExpression methodCallExpression))
                return false;
            return methodCallExpression.Method.Name.InSet("StartsWith", "EndsWith", "IsMatch");
        }

        private static string BuildContainsExpression(MethodCallExpression expression)
        {
            string propertyPath;
            IEnumerable<string> values;
            if (expression.Method.Name == "Contains")
            {
                if(typeof(IList).IsAssignableFrom(expression.Method.DeclaringType))
                {
                    var list = GetInstanceFromExpression(expression.Object);
                    propertyPath = ExtractPath(expression.Arguments.Single());
                    values = (list as IEnumerable).Cast<object>().Select(obj => $"'{obj}'");
                }
                else if (expression.Method.DeclaringType == typeof(Enumerable))
                {
                    var collection = GetInstanceFromExpression(expression.Arguments[0]);
                    propertyPath = ExtractPath(expression.Arguments[1]);
                    values = (collection as IEnumerable).Cast<object>().Select(obj => $"'{obj}'");
                }
                else if (expression.Method.DeclaringType == typeof(string))
                {
                    propertyPath = ExtractPath(expression.Object);
                    var searchTerm = GetValue(expression.Arguments.Single(), omitQuotes: true);
                    return $"{propertyPath} LIKE '%{searchTerm}%'";
                }
                else
                    throw new NotSupportedException($"Cannot build IN-expression from method '{expression.Method.Name}'");
            }
            else if (expression.Method.Name == "InSet")
            {
                propertyPath = ExtractPath(expression.Arguments[0]);
                values = (GetInstanceFromExpression(expression.Arguments[1]) as IList).Cast<object>().Select(obj => $"'{obj}'");
            }
            else
                throw new NotSupportedException($"Cannot build IN-expression from method '{expression.Method.Name}'");
            return $"{propertyPath} IN [{string.Join(", ", values)}]";
        }

        private static string BuildStringMatchStatement(MethodCallExpression expression)
        {
            string propertyPath;
            string matchPattern;
            var methodName = expression.Method.Name;
            if (methodName == "StartsWith")
            {
                propertyPath = ExtractPath(expression.Object);
                matchPattern = $"'{GetValue(expression.Arguments[0], omitQuotes: true)}%'";
            } 
            else if (methodName == "EndsWith")
            {
                propertyPath = ExtractPath(expression.Object);
                matchPattern = $"'%{GetValue(expression.Arguments[0], omitQuotes: true)}'";
            } 
            else if (methodName == "IsMatch")
            {
                propertyPath = ExtractPath(expression.Arguments[0]);
                matchPattern = GetValue(expression.Arguments[1], true);
            }
            else
                throw new NotSupportedException($"Cannot build LIKE-expression for method '{methodName}'");
            return $"{propertyPath} LIKE {matchPattern}";
        }

        private static string GetValue(Expression expression, bool omitQuotes = false)
        {
            var constant = GetInstanceFromExpression(expression);
            if (constant == null)
                return "null";
            var isNumber = constant.IsNumber();
            if (isNumber || omitQuotes)
                return $"{constant}";
            return $"'{constant}'";
        }

        private static object GetInstanceFromExpression(Expression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            var constant = getter();
            return constant;
        }

        public static string ExtractPath(Expression expression)
        {
            switch (expression)
            {
                case UnaryExpression unaryExpression:
                    if (unaryExpression.NodeType.InSet(ExpressionType.Convert, ExpressionType.ConvertChecked, ExpressionType.Quote))
                        return ExtractPath(unaryExpression.Operand);
                    throw new NotSupportedException($"Unsupported unary operator '{unaryExpression.NodeType}'");
                case LambdaExpression lambdaExpression:
                    return ExtractPath(lambdaExpression.Body);
                case MemberExpression memberExpression:
                    return ExtractPath(memberExpression.Expression) + "." + GetMemberName(memberExpression.Member);
                case ParameterExpression parameterExpression:
                    return "Data";
                case MethodCallExpression methodCallExpression:
                    if (methodCallExpression.Method.Name == "get_Item")
                    {
                        var argumentExpression = methodCallExpression.Arguments.Single();
                        var index = (int) ((ConstantExpression) argumentExpression).Value;
                        var arrayAccess = index < 0 ? "" : "." + index;
                        return ExtractPath(methodCallExpression.Object) +  arrayAccess;
                    }
                    throw new NotSupportedException("Method calls are not supported");
                default:
                    throw new NotSupportedException(expression.GetType().Name);
            }
        }

        private static string GetMemberName(MemberInfo memberInfo)
        {
            var jsonPropertyAttribute = memberInfo.GetCustomAttribute<JsonPropertyAttribute>();
            if (jsonPropertyAttribute == null)
                return memberInfo.Name;
            return jsonPropertyAttribute.PropertyName;
        }

        private static string ParseBinaryExpression(BinaryExpression expression, string operatorSymbol, bool omitParentheses = false)
        {
            var leftExpression = IsPathReference(expression.Left)
                ? ExtractPath(expression.Left)
                : ParseWhereExpression(expression.Left);
            var rightExpression = IsPathReference(expression.Right)
                ? ExtractPath(expression.Right)
                : ParseWhereExpression(expression.Right);
            if(omitParentheses)
                return $"{leftExpression} {operatorSymbol} {rightExpression}";
            else
                return $"({leftExpression}) {operatorSymbol} ({rightExpression})";
        }

        private static bool IsPathReference(Expression expression)
        {
            var currentExpression = expression;
            while (currentExpression is MemberExpression memberExpression)
            {
                currentExpression = memberExpression.Expression;
            }
            return currentExpression is ParameterExpression;
        }
    }
}
