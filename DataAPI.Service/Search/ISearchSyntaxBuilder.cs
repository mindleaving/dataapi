using System.Collections.Generic;

namespace DataAPI.Service.Search
{
    public interface ISearchSyntaxBuilder
    {
        string BuildLogicalOperatorExpression(IEnumerable<string> relationalExpressions, string logicalOperator);
        string BuildRelationshipExpression(string fieldName, string relationshipOperator, string value);
        string BuildUnaryExpression(string fieldName, string unaryExpression);
        string EncapsulateSubExpression(string expression);
    }
}
