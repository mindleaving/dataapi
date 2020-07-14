using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataAPI.Client
{
    internal class DataApiQueryable<T> : IOrderedQueryable<T>
    {
        public DataApiQueryable(DataApiQueryProvider<T> provider, Expression expression)
        {
            this.provider = provider;
            Expression = expression;
            ElementType = typeof(T);
        }

        public Type ElementType { get; }
        public Expression Expression { get; }
        private readonly DataApiQueryProvider<T> provider;
        public IQueryProvider Provider => provider;

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public override string ToString()
        {
            return provider.GetQueryText(Expression);
        }
    }
}