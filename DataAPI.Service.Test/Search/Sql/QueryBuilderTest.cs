using System.Text.RegularExpressions;
using DataAPI.Service.Search;
using DataAPI.Service.Search.Sql;
using NUnit.Framework;

namespace DataAPI.Service.Test.Search.Sql
{
    [TestFixture]
    public class QueryBuilderTest
    {
        [Test]
        [TestCase(
            "SELECT * FROM Component WHERE Data.is_deleted = false AND Data.source_system = 'Ingredients' AND (Data.source_id LIKE '%skim%' OR Data.business_name LIKE '%skim%')",
            "SELECT * FROM Component WHERE is_deleted = 0 AND source_system = 'Ingredients' AND (source_id LIKE '%skim%' OR business_name LIKE '%skim%')"
        )]
        [TestCase(
            "SELECT * FROM Component WHERE Data.is_deleted = false OR Data.source_system = 'Ingredients' OR Data.source_id LIKE '%skim%' AND Data.business_name LIKE '%skim%'",
            "SELECT * FROM Component WHERE is_deleted = 0 OR source_system = 'Ingredients' OR (source_id LIKE '%skim%' AND business_name LIKE '%skim%')"
        )]
        [TestCase(
            "SELECT * FROM Component WHERE Data.is_deleted = false AND Data.source_system = 'Ingredients' AND Data.source_id LIKE '%skim%' OR Data.business_name LIKE '%skim%'",
            "SELECT * FROM Component WHERE ((is_deleted = 0 AND source_system = 'Ingredients') AND source_id LIKE '%skim%') OR business_name LIKE '%skim%'"
        )]
        [TestCase(
            "SELECT * FROM Component WHERE (Data.is_deleted = false OR Data.source_system = 'Ingredients') AND (Data.source_id LIKE '%skim%' OR Data.business_name LIKE '%skim%')",
            "SELECT * FROM Component WHERE (is_deleted = 0 OR source_system = 'Ingredients') AND (source_id LIKE '%skim%' OR business_name LIKE '%skim%')"
        )]
        [TestCase(
            "SELECT * FROM Component WHERE Data.is_deleted = false AND (Data.source_system = 'Ingredients' OR (Data.source_id LIKE '%skim%' OR Data.business_name LIKE '%skim%'))",
            "SELECT * FROM Component WHERE is_deleted = 0 AND (source_system = 'Ingredients' OR (source_id LIKE '%skim%' OR business_name LIKE '%skim%'))"
        )]
        public void QueryAsExpected(string input, string expected)
        {
            var sut = new QueryBuilder(x => x.Replace("Data.", ""));
            var parsedQuery = DataApiSqlQueryParser.Parse(input);
            var actual = sut.Build(parsedQuery, parsedQuery.FromArguments);
            var whitespaceNormalizedActual = Regex.Replace(actual, "\\s+", " ");
            Assert.That(whitespaceNormalizedActual, Is.EqualTo(expected));
        }
    }
}
