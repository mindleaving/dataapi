using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Commons.Extensions;

namespace DataAPI.Service.Search.Sql
{
    public class SqlSearchSyntaxBuilder : ISearchSyntaxBuilder
    {
        private readonly Func<string, string> fieldNameManipulationFunc;

        public SqlSearchSyntaxBuilder(Func<string,string> fieldNameManipulationFunc)
        {
            this.fieldNameManipulationFunc = fieldNameManipulationFunc;
        }

        public string BuildLogicalOperatorExpression(IEnumerable<string> relationalExpressions, string logicalOperator)
        {
            switch (logicalOperator.ToUpperInvariant().Trim())
            {
                case "AND":
                case "&&":
                    return string.Join(" AND ", relationalExpressions);
                case "OR":
                case "||":
                    return string.Join(" OR ", relationalExpressions);
                default:
                    throw new NotSupportedException($"Logical operator '{logicalOperator}' is not supported");
            }
        }

        public string BuildRelationshipExpression(string fieldName, string relationshipOperator, string value)
        {
            var operatorToken = TranslateRelationshipOpertor(relationshipOperator);
            var manipulatedFieldName = fieldNameManipulationFunc(fieldName);
            var manipulatedValue = value.Trim();
            if (operatorToken.InSet("IN", "NOT IN"))
            {
                if (manipulatedValue.StartsWith("[") && manipulatedValue.EndsWith("]"))
                    manipulatedValue = $"({manipulatedValue.Substring(1, manipulatedValue.Length - 2)})";
                if (Regex.IsMatch(manipulatedValue, "\\(\\s*\\)"))
                    return "1 = 0";
            }

            if (manipulatedValue.ToLowerInvariant() == "false")
                manipulatedValue = "0";
            else if (manipulatedValue.ToLowerInvariant() == "true")
                manipulatedValue = "1";
            return $"{manipulatedFieldName} {operatorToken} {manipulatedValue}";
        }

        private string TranslateRelationshipOpertor(string relationshipOperator)
        {
            switch (relationshipOperator.ToUpperInvariant().Trim())
            {
                case "=":
                case "==":
                case "IS":
                    return "=";
                case "!=":
                case "<>":
                case "IS NOT":
                    return "<>";
                case ">":
                    return ">";
                case "<":
                    return "<";
                case ">=":
                    return ">=";
                case "<=":
                    return "<=";
                case "LIKE":
                    return "LIKE";
                case "IN":
                    return "IN";
                case "NOT IN":
                    return "NOT IN";
                default:
                    throw new NotSupportedException($"Unsupported reltationship operator '{relationshipOperator}'");
            }
        }

        public string BuildUnaryExpression(string fieldName, string unaryExpression)
        {
            var operatorToken = TranslateUnaryOperator(unaryExpression);
            var manipulatedFieldName = fieldNameManipulationFunc(fieldName);
            return $"{manipulatedFieldName} {operatorToken}";
        }

        public string EncapsulateSubExpression(string expression)
        {
            return $"({expression})";
        }

        public string TranslateUnaryOperator(string unaryOperator)
        {
            switch (unaryOperator.ToUpperInvariant().Trim())
            {
                case "EXISTS":
                    return "IS NOT NULL";
                case "NOT EXISTS":
                    return "IS NULL";
                default:
                    throw new NotSupportedException($"Unsupported unary operator '{unaryOperator}'");
            }
        }
    }
}