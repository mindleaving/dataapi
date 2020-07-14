using DataAPI.Service.Search.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace DataAPI.Service.Test
{
    [TestFixture]
    public class SkipDefinitionBuilderTest
    {
        [Test]
        public void PipelineDefinitionCanBeBuild()
        {
            var skipArguments = "31";
            JsonPipelineStageDefinition<BsonDocument, BsonDocument> stage = null;
            Assert.That(() => stage = SkipDefinitionBuilder.Build<BsonDocument>(skipArguments), Throws.Nothing);
            Assert.That(stage, Is.Not.Null);
        }
    }
}
