using DataAPI.Service.Search.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace DataAPI.Service.Test
{
    [TestFixture]
    public class ProjectionDefinitionBuilderTest
    {
        [Test]
        public void PipelineDefinitionCanBeBuild()
        {
            var selectArguments = "id, test.subtest, product.price.tax as tax";
            JsonPipelineStageDefinition<BsonDocument, BsonDocument> stage = null;
            Assert.That(() => stage = ProjectionDefinitionBuilder.Build<BsonDocument>(selectArguments), Throws.Nothing);
            Assert.That(stage, Is.Not.Null);
            Assert.That(stage.Json, Is.EqualTo("{ $project : { \"id\" : true, \"test_subtest\" : \"$test.subtest\", \"tax\" : \"$product.price.tax\" } }"));
        }
    }
}
