using DataAPI.Service.Search.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace DataAPI.Service.Test
{
    [TestFixture]
    public class GroupDefinitionBuilderTest
    {
        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void EmptyArgumentsReturnNull(string groupByArguments)
        {
            JsonPipelineStageDefinition<BsonDocument, BsonDocument> stage = null;
            Assert.That(() => stage = GroupDefinitionBuilder.Build<BsonDocument>(groupByArguments), Throws.Nothing);
            Assert.That(stage, Is.Null);
        }

        [Test]
        [TestCase("price")]
        public void PipelineDefinitionCanBeBuild(string groupByArguments)
        {
            JsonPipelineStageDefinition<BsonDocument, BsonDocument> stage = null;
            Assert.That(() => stage = GroupDefinitionBuilder.Build<BsonDocument>(groupByArguments), Throws.Nothing);
            Assert.That(stage, Is.Not.Null);
        }
    }
}
