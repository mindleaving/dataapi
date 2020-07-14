using System;
using System.Collections.Generic;

namespace DataAPI.Service.Search
{
    public class ExpressionChain
    {
        public ExpressionChain(IList<ISearchExpression> expressions, IList<string> operators)
        {
            if(expressions.Count != operators.Count+1)
                throw new ArgumentException("There must be exactly one more expression than operators");
            Expressions = expressions;
            Operators = operators;
        }

        public IList<ISearchExpression> Expressions { get; }
        public IList<string> Operators { get; }
    }
}