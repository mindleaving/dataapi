using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataAPI.Client.Serialization;
using DataAPI.DataStructures;
using Newtonsoft.Json.Linq;

namespace DataAPI.Client
{
    internal class DataApiQueryProvider<TSource> : IQueryProvider
    {
        private readonly IDataApiClient dataApiClient;
        private readonly ExpressionParser expressionParser;

        public DataApiQueryProvider(IDataApiClient dataApiClient)
        {
            this.dataApiClient = dataApiClient;
            expressionParser = new ExpressionParser();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new DataApiQueryable<TSource>(this, expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if(typeof(TSource) == typeof(TElement))
                return new DataApiQueryable<TElement>(this as DataApiQueryProvider<TElement>, expression);

            if(!(expression is MethodCallExpression callExpression) || callExpression.Method.Name != nameof(Queryable.Select))
                throw new NotSupportedException();

            var jObjects = (List<JObject>)Execute(callExpression.Arguments.First());
            var castedJObjects = jObjects.Select(jObject => jObject["Data"].ToObject<TSource>());
            var selectorExpression = callExpression.Arguments[1];

            LambdaExpression selectorLambdaExpression;
            if (selectorExpression is LambdaExpression)
                selectorLambdaExpression = (LambdaExpression) selectorExpression;
            else if (selectorExpression is UnaryExpression unaryExpression && unaryExpression.Operand is LambdaExpression)
                selectorLambdaExpression = (LambdaExpression)unaryExpression.Operand;
            else
                throw new NotSupportedException();
            if(selectorLambdaExpression.Parameters.Count == 0
               || selectorLambdaExpression.Parameters.First().Type != typeof(TSource))
                throw new NotSupportedException();

            if (selectorLambdaExpression.Parameters.Count == 1)
            {
                Expression<Func<TSource, TElement>> selector = Expression.Lambda<Func<TSource, TElement>>(
                    selectorLambdaExpression.Body,
                    selectorLambdaExpression.Parameters.First());
                return castedJObjects.AsQueryable().Select(selector);
            } 
            else if (selectorLambdaExpression.Parameters.Count == 2)
            {
                Expression<Func<TSource, int, TElement>> selector = Expression.Lambda<Func<TSource, int, TElement>>(
                    selectorLambdaExpression.Body,
                    selectorLambdaExpression.Parameters.First());
                return castedJObjects.AsQueryable().Select(selector);
            }
            else 
                throw new ArgumentException($"Expression is insufficient to convert from '{typeof(TSource)}' to '{typeof(TElement)}'");
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var resultType = typeof(TResult);
            var isStrongTypeCollection = typeof(IEnumerable<TSource>).IsAssignableFrom(resultType);
            var isJObjectCollection = typeof(IEnumerable<JObject>).IsAssignableFrom(resultType);
            var isStrongTypeObject = resultType == typeof(TSource);
            var isJObject = resultType == typeof(JObject);
            if(isStrongTypeObject || isJObject)
                throw new NotSupportedException("Single objects are currently not supported when executing queryable");

            if (!isStrongTypeCollection && !isJObjectCollection)
                throw new NotSupportedException($"Incompatible return type for {GetType().Name}.Execute '{typeof(TResult).Name}'");

            var query = expressionParser.ParseQueryExpression(expression);
            var jObjects = Task.Run(
                async () =>
                {
                    var resultStream = await dataApiClient.SearchAsync(query, ResultFormat.Json);
                    return await resultStream.ReadAllSearchResultsAsync();
                }).Result;
            if (isJObjectCollection)
                return (TResult) jObjects.AsEnumerable();
            else if(isStrongTypeCollection)
                return (TResult)jObjects.Select(x => x["Data"].ToObject<TSource>());
            else
                throw new Exception();
        }

        public object Execute(Expression expression)
        {
            var query = expressionParser.ParseQueryExpression(expression);
            return Task.Run(
                async () =>
                {
                    var resultStream = await dataApiClient.SearchAsync(query, ResultFormat.Json);
                    return await resultStream.ReadAllSearchResultsAsync();
                }).Result;
        }

        public string GetQueryText(Expression expression)
        {
            return expressionParser.ParseQueryExpression(expression);
        }
    }
}