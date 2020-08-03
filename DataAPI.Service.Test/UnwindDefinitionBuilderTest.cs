using System.Collections.Generic;
using System.Linq;
using DataAPI.Service.Search.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace DataAPI.Service.Test
{
    [TestFixture]
    public class UnwindDefinitionBuilderTest
    {
        [Test]
        public void AsProjectionIsIgnored()
        {
            // Ignore "AS OtherFieldName" projections in SELECT clause
            var selectArguments = "Data.Id AS Id, Data.Products.Price as Price";
            List<JsonPipelineStageDefinition<BsonDocument, BsonDocument>> stages = null;
            Assert.That(() => stages = UnwindDefinitionBuilder.Build<BsonDocument>(selectArguments).ToList(), Throws.Nothing);
            Assert.That(stages, Is.Not.Null);
            Assert.That(stages.Count, Is.EqualTo(4));
            Assert.That(stages[0].Json, Is.EqualTo("{ $unwind : \"$Data\" }"));
            Assert.That(stages[1].Json, Is.EqualTo("{ $unwind : \"$Data.Id\" }"));
            Assert.That(stages[2].Json, Is.EqualTo("{ $unwind : \"$Data.Products\" }"));
            Assert.That(stages[3].Json, Is.EqualTo("{ $unwind : \"$Data.Products.Price\" }"));
        }
    }
}
