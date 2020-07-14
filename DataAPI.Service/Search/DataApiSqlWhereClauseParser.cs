using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataAPI.Service.Helpers;

namespace DataAPI.Service.Search
{
    public class DataApiSqlWhereClauseParser
    {
        private readonly DataApiSqlWhereClauseValidator validator;
        private readonly DataApiSqlWhereClauseSplitter splitter;
        private readonly ISearchSyntaxBuilder syntaxBuilder;

        public DataApiSqlWhereClauseParser(ISearchSyntaxBuilder syntaxBuilder)
        {
            validator = new DataApiSqlWhereClauseValidator();
            splitter = new DataApiSqlWhereClauseSplitter();
            this.syntaxBuilder = syntaxBuilder;
        }

        public string Parse(string whereArguments)
        {
            if (string.IsNullOrWhiteSpace(whereArguments))
                return null;
            if(!validator.IsValid(whereArguments, out var errorText))
                throw new FormatException(errorText);
            return ParseExpressions(whereArguments);
        }

        private string ParseExpressions(string whereArguments)
        {
            var expressionChain = splitter.SplitIntoExpressions(whereArguments, DataApiSqlOperators.LogicalOperators);
            if(expressionChain.Expressions.Any(x => string.IsNullOrEmpty(x.Value)))
                throw new FormatException("Empty expression (duplicate logical operator?)");
            if (expressionChain.Expressions.Count == 1)
            {
                if(!(expressionChain.Expressions.Single() is SearchExpression expression))
                    throw new Exception("Single expression in expression chain is expected to be non-subexpression");
                return ParseExpression(expression);
            }
            var operatorPriorities = expressionChain.Operators.Select(GetLogicalOperatorPriority).ToList();
            var highestOperatorPriority = operatorPriorities.Max();
            var loewstOperatorPriority = operatorPriorities.Min();
            while (highestOperatorPriority != loewstOperatorPriority)
            {
                expressionChain = CombineHighestPriorityLogicalOperatorsIntoSubExpressions(expressionChain);
                operatorPriorities = expressionChain.Operators.Select(GetLogicalOperatorPriority).ToList();
                highestOperatorPriority = operatorPriorities.Max();
                loewstOperatorPriority = operatorPriorities.Min();
            }

            // Assume that different operators have different priorities
            // and hence that all having the same priority means we only have one type of operator, e.g. OR
            var recursedExpressions = new List<string>();
            foreach (var expression in expressionChain.Expressions)
            {
                if(expression is SearchExpression topLevelExpression)
                    recursedExpressions.Add(ParseExpression(topLevelExpression));
                else if(expression is SubSearchExpression subExpression)
                    recursedExpressions.Add(syntaxBuilder.EncapsulateSubExpression(ParseExpressions(subExpression.Value)));
                else
                    throw new NotImplementedException();
            }
            return syntaxBuilder.BuildLogicalOperatorExpression(recursedExpressions, expressionChain.Operators.First()); // <- Assumption used here
        }

        private static ExpressionChain CombineHighestPriorityLogicalOperatorsIntoSubExpressions(ExpressionChain expressionChain)
        {
            var operatorPriorities = expressionChain.Operators.Select(GetLogicalOperatorPriority).ToList();
            var highestOperatorPriority = operatorPriorities.Max();
            var newExpressions = new List<ISearchExpression>();
            var newOperators = new List<string>();
            var leftExpression = expressionChain.Expressions[0];
            for (var operatorIndex = 0; operatorIndex < expressionChain.Operators.Count; operatorIndex++)
            {
                var logicalOperator = expressionChain.Operators[operatorIndex];
                var operatorPriority = GetLogicalOperatorPriority(logicalOperator);
                var rightExpression = expressionChain.Expressions[operatorIndex + 1];
                if (operatorPriority == highestOperatorPriority)
                {
                    leftExpression = CombineExpressions(leftExpression, logicalOperator, rightExpression);
                }
                else
                {
                    newExpressions.Add(leftExpression);
                    newOperators.Add(logicalOperator);
                    leftExpression = rightExpression;
                }
            }
            newExpressions.Add(leftExpression);
            return new ExpressionChain(newExpressions, newOperators);
        }

        private static SubSearchExpression CombineExpressions(ISearchExpression leftExpression, string logicalOperator, ISearchExpression rightExpression)
        {
            var subExpressionString = string.Empty;
            if (leftExpression is SubSearchExpression)
                subExpressionString += $"({leftExpression.Value})";
            else
                subExpressionString += leftExpression.Value;
            subExpressionString += $" {logicalOperator} ";
            if (rightExpression is SubSearchExpression)
                subExpressionString += $"({rightExpression.Value})";
            else
                subExpressionString += rightExpression.Value;
            return new SubSearchExpression(subExpressionString);
        }

        private static int GetLogicalOperatorPriority(string logicalOperator)
        {
            switch (logicalOperator.ToUpperInvariant())
            {
                case "AND":
                case "&&":
                    return 2;
                case "OR":
                case "||":
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logicalOperator));
            }
        }

        private string ParseExpression(ISearchExpression searchExpression)
        {
            var preprocessedExpressionValue = PreprocessExpression(searchExpression.Value);
            if (TryMatchUnaryOperator(preprocessedExpressionValue, out var unaryOperatorExpression))
                return unaryOperatorExpression;
            if (TryMatchRelationshipOperator(preprocessedExpressionValue, out var relationshipExpression))
                return relationshipExpression;
            throw new FormatException($"Invalid filter expression '{searchExpression.Value}'");
        }

        private static string PreprocessExpression(string expressionValue)
        {
            return Regex.Replace(expressionValue, "\\s+", " ").Trim();
        }

        private bool TryMatchUnaryOperator(string expressionValue, out string unaryExpression)
        {
            unaryExpression = null;
            var leftPattern = BuildFieldNameRegexPattern();
            var aggregatedUnaryOperators = DataApiSqlOperators.UnaryOperators.Aggregate((a, b) => a + "|" + b);
            var combinedPattern = $"^{leftPattern}\\s*({aggregatedUnaryOperators})$";
            var regex = Regex.Match(expressionValue, combinedPattern, RegexOptions.IgnoreCase);
            if (!regex.Success)
                return false;
            var unaryOperator = regex.Groups[2].Value;
            var fieldName = ParseHelpers.StripQuotes(regex.Groups[1].Value);
            unaryExpression = syntaxBuilder.BuildUnaryExpression(fieldName, unaryOperator.Trim());
            return true;
        }

        private bool TryMatchRelationshipOperator(string expressionValue, out string relationshipExpression)
        {
            relationshipExpression = null;
            var leftPattern = BuildFieldNameRegexPattern();
            var rightPattern = BuildValueRegexPattern();
            var aggregatedRelationshipOperators = DataApiSqlOperators.RelationshipOperators.Aggregate((a, b) => a + "|" + b);
            var combinedPattern = $"^{leftPattern}\\s*({aggregatedRelationshipOperators})\\s*{rightPattern}$";
            var regex = Regex.Match(expressionValue, combinedPattern, RegexOptions.IgnoreCase);
            if (!regex.Success)
                return false;
            var relationshipOperator = regex.Groups[2].Value;
            var fieldName = ParseHelpers.StripQuotes(regex.Groups[1].Value);
            var value = regex.Groups[3].Value;
            relationshipExpression = syntaxBuilder.BuildRelationshipExpression(fieldName, relationshipOperator.Trim(), value);
            return true;
        }

        private static string BuildFieldNameRegexPattern()
        {
            var quotes = "\"'";
            var unquotedExpressionPattern = $"[^{quotes}\\s]+";
            var quotedStringPattern = $"[{quotes}][^{quotes}]*[{quotes}]";
            return $"({unquotedExpressionPattern}|{quotedStringPattern})";
        }

        private static string BuildValueRegexPattern()
        {
            var quotes = "\"'";
            var unquotedExpressionPattern = $"[^{quotes}\\s]+";
            var quotedStringPattern = $"[{quotes}][^{quotes}]*[{quotes}]";
            var listPattern = "[(\\[][^\\])]*[\\])]";
            return $"({unquotedExpressionPattern}|{quotedStringPattern}|{listPattern})";
        }
    }
}
