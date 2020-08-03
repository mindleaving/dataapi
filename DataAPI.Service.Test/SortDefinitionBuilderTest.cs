using DataAPI.Service.Search.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace DataAPI.Service.Test
{
    [TestFixture]
    public class SortDefinitionBuilderTest
    {
        [Test]
        public void PipelineDefinitionCanBeBuild()
        {
            var orderByArguments = "index ASC, timestamp DESC";
            JsonPipelineStageDefinition<BsonDocument, BsonDocument> stage = null;
            Assert.That(() => stage = SortDefinitionBuilder.Build<BsonDocument>(orderByArguments), Throws.Nothing);
            Assert.That(stage, Is.Not.Null);
        }
    }
}
