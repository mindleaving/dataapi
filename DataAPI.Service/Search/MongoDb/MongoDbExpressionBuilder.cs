using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Commons.Extensions;
using DataAPI.Service.Helpers;

namespace DataAPI.Service.Search.MongoDb
{
    public class MongoDbExpressionBuilder : ISearchSyntaxBuilder
    {
        public string BuildLogicalOperatorExpression(IEnumerable<string> relationalExpressions, string logicalOperator)
        {
            var aggregatedExpressions = string.Join(", ", relationalExpressions);
            switch (logicalOperator.ToUpperInvariant())
            {
                case "AND":
                case "&&":
                    return $"{{ $and : [ {aggregatedExpressions} ] }}";
                case "OR":
                case "||":
                    return $"{{ $or : [ {aggregatedExpressions} ] }}";
                default:
                    throw new NotSupportedException($"Logical operator '{logicalOperator}' is not supported");
            }
        }

        public string BuildRelationshipExpression(string fieldName, string relationshipOperator, string value)
        {
            var operatorToken = TranslateRelationshipOpertor(relationshipOperator);
            var manipulatedValue = value.Trim();
            if (operatorToken == "$regex")
            {
                var regexPattern = ParseHelpers.HasQuotes(value)
                    ? Regex.Escape(ParseHelpers.StripQuotes(value)).Replace(@"\", @"\\").Replace("%", ".*").Replace("_", ".")
                    : value;
                manipulatedValue = $"'^{regexPattern}$', $options: 'i'";
            }
            else if (operatorToken.InSet("$in", "$nin"))
            {
                if (manipulatedValue.StartsWith("(") && manipulatedValue.EndsWith(")"))
                    manipulatedValue = $"[{manipulatedValue.Substring(1, manipulatedValue.Length - 2)}]";
            }
            else if (DateTime.TryParse(ParseHelpers.StripQuotes(value), out var parsedTimestamp))
                manipulatedValue = $"ISODate('{parsedTimestamp:yyyy-MM-ddTHH:mm:ssZ}')";
            else if (value.ToLowerInvariant().InSet("null", "true", "false"))
                manipulatedValue = manipulatedValue.ToLowerInvariant();
            return $"{{ \"{fieldName}\" : {{ {operatorToken} : {manipulatedValue} }} }}";
        }

        private string TranslateRelationshipOpertor(string relationshipOperator)
        {
            switch (relationshipOperator.ToUpperInvariant().Trim())
            {
                case "=":
                case "==":
                case "IS":
                    return "$eq";
                case "!=":
                case "<>":
                case "IS NOT":
                    return "$ne";
                case ">":
                    return "$gt";
                case "<":
                    return "$lt";
                case ">=":
                    return "$gte";
                case "<=":
                    return "$lte";
                case "LIKE":
                    return "$regex";
                case "IN":
                    return "$in";
                case "NOT IN":
                    return "$nin";
                default:
                    throw new NotSupportedException($"Unsupported reltationship operator '{relationshipOperator}'");
            }
        }

        public string BuildUnaryExpression(string fieldName, string unaryExpression)
        {
            var operatorExpression = TranslateUnaryOperator(unaryExpression);
            return  $"{{ \"{fieldName}\" : {operatorExpression} }}";
        }

        public string EncapsulateSubExpression(string expression)
        {
            return expression; // No encapsulation needed because encapsulated already by { ... }
        }

        private string TranslateUnaryOperator(string unaryOperator)
        {
            switch (unaryOperator.ToUpperInvariant().Trim())
            {
                case "EXISTS":
                    return "{ $exists : true }";
                case "NOT EXISTS":
                    return "{ $exists : false }";
                default:
                    throw new NotSupportedException($"Unsupported unary operator '{unaryOperator}'");
            }
        }
    }
}