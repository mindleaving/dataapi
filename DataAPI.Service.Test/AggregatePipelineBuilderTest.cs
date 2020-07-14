using DataAPI.Service.Search;
using DataAPI.Service.Search.MongoDb;
using NUnit.Framework;

namespace DataAPI.Service.Test
{
    [TestFixture]
    public class AggregatePipelineBuilderTest
    {
        [Test]
        [Ignore("MongoDB driver translates regex expressions to other format and hence test is inconsistent")]
        [TestCase("SELECT * WHERE Id LIKE '48%' LIMIT 3 FROM Image")]
        public void LikeClauseIsCorrectlyTranslated(string query)
        {
            var parsedQuery = DataApiSqlQueryParser.Parse(query);
            var actual = AggregatePipelineBuilder.Build(parsedQuery);
            Assert.That(actual.ToString(), Contains.Substring("$regex"));
        }
    }
}
