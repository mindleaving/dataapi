using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Commons.Extensions;

namespace DataAPI.Service.Search
{
    public class DataApiSqlWhereClauseSplitter
    {
        public ExpressionChain SplitIntoExpressions(string stripped, string[] keywords)
        {
            var topLevelClause = string.Empty;
            var subExpressions = new List<SubSearchExpression>();
            var openParenthesisCount = 0;
            var currentSubExpression = string.Empty;
            const string SubExpressionKeyword = "subexpression";
            var subexpressionPattern = $@"\[{SubExpressionKeyword}(?<idx>[0-9]+)\]";
            if(Regex.IsMatch(stripped, $".*{subexpressionPattern}.*"))
                throw new FormatException(); // Query contains our magic subexpression keyword
            foreach (var c in stripped)
            {
                if (c == '(')
                {
                    openParenthesisCount++;
                    if(openParenthesisCount == 1)
                        continue;
                }
                if (c == ')')
                {
                    openParenthesisCount--;
                    if (openParenthesisCount == 0)
                    {
                        topLevelClause += $"[{SubExpressionKeyword}{subExpressions.Count}]";
                        subExpressions.Add(new SubSearchExpression(currentSubExpression));
                        currentSubExpression = string.Empty;
                        continue;
                    }
                }
                var isSubExpression = openParenthesisCount > 0;
                if (isSubExpression)
                {
                    currentSubExpression += c;
                }
                else
                {
                    topLevelClause += c;
                }
            }

            var topLevelExpressions = topLevelClause.Split(keywords, StringSplitOptions.None).Select(x => x.Trim()).ToList();
            var expressions = new List<ISearchExpression>();
            foreach (var topLevelExpression in topLevelExpressions)
            {
                var pureSubExpressionMatch = Regex.Match(topLevelExpression, $"^{subexpressionPattern}$");
                if(pureSubExpressionMatch.Success)
                {
                    var subExpressionIdx = int.Parse(pureSubExpressionMatch.Groups["idx"].Value);
                    expressions.Add(subExpressions[subExpressionIdx]);
                }
                else
                {
                    var partialSubExpressionMatch = Regex.Match(topLevelExpression, subexpressionPattern);
                    if (partialSubExpressionMatch.Success)
                    {
                        var subExpressionIdx = int.Parse(partialSubExpressionMatch.Groups["idx"].Value);
                        var subExpression = subExpressions[subExpressionIdx];
                        var subExpressionReplaced = Regex.Replace(
                            topLevelExpression, 
                            subexpressionPattern,
                            $"({subExpression.Value})");
                        expressions.Add(new SearchExpression(subExpressionReplaced));
                    }
                    else
                    {
                        expressions.Add(new SearchExpression(topLevelExpression));
                    }
                }
            }

            var detectedKeywords = keywords
                .SelectMany(keyword => topLevelClause.AllIndicesOf(keyword).Select(idx => new
                {
                    Keyword = keyword.Trim(),
                    Index = idx
                }))
                .OrderBy(x => x.Index)
                .Select(x => x.Keyword)
                .ToList();
            var candidateChain = new ExpressionChain(expressions, detectedKeywords);
            if (!candidateChain.Operators.Any() 
                   && candidateChain.Expressions.Count == 1
                   && candidateChain.Expressions.Single() is SubSearchExpression)
            {
                return SplitIntoExpressions(candidateChain.Expressions.Single().Value, DataApiSqlOperators.LogicalOperators);
            }
            return candidateChain;
        }
    }
}
