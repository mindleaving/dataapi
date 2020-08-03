using DataAPI.Service.Search.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace DataAPI.Service.Test
{
    [TestFixture]
    public class LimitDefinitionBuilderTest
    {
        [Test]
        public void PipelineDefinitionCanBeBuild()
        {
            var limitArguments = "31";
            JsonPipelineStageDefinition<BsonDocument, BsonDocument> stage = null;
            Assert.That(() => stage = LimitDefinitionBuilder.Build<BsonDocument>(limitArguments), Throws.Nothing);
            Assert.That(stage, Is.Not.Null);
        }
    }
}
