using System.Linq;
using DataAPI.Service.Search;
using NUnit.Framework;

namespace DataAPI.Service.Test.Search
{
    [TestFixture]
    public class DataApiSqlWhereClauseSplitterTest
    {
        [Test]
        public void ParenthesesArePreserved()
        {
            var input = "Data.is_deleted = false AND Data.source_system = 'Ingredients' AND (Data.source_id LIKE '%skim%' OR Data.business_name LIKE '%skim%')";
            var sut = new DataApiSqlWhereClauseSplitter();

            var actual = sut.SplitIntoExpressions(input, new[] {"AND", "OR"});
            Assert.That(actual.Expressions.Count, Is.EqualTo(3));
            Assert.That(actual.Expressions.Last(), Is.TypeOf<SubSearchExpression>());
            Assert.That(actual.Operators, Is.EqualTo(new[] { "AND", "AND" }));
        }
    }
}
