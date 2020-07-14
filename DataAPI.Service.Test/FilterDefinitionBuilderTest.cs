using System;
using DataAPI.Service.Search.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace DataAPI.Service.Test
{
    [TestFixture]
    public class FilterDefinitionBuilderTest
    {
        [Test]
        [TestCase("price > 300")]
        [TestCase(" price > 300")]
        [TestCase("price > 300 ")]
        [TestCase("price = 300")]
        [TestCase("price \t=   300 ")]
        [TestCase("price == 300 ")]
        [TestCase("price=300")]
        [TestCase("price IS 300")]
        [TestCase("price is 300")]
        [TestCase("price Is 300")]
        [TestCase("Data.MyProperty IS NOT NULL")]
        [TestCase("Data.MyProperty is not null")]
        [TestCase("Data.MyProperty is Not NULL")]
        [TestCase("Data.MyProperty  \t IS   NOT   NULL")]
        [TestCase("Data.ExistingProperty EXISTS")]
        [TestCase("Data.ExistingProperty  \t EXISTS")]
        [TestCase("Data.NonExistingProperty NOT EXISTS")]
        [TestCase("(timestamp > '2018-01-01' AND (name LIKE '%Test%' || Id = 123) OR price > 300)")]
        [TestCase("Data.Recipient = 'jdoe' OR Data.Requester = 'jdoe'")]
        [TestCase(" ((((Data.Id = 'MyID') AND ((Data.Number1 = 2) OR (Data.Number2 < 3))))) ")]
        [TestCase("Data.Recipient IN ('jdoe','auser','buser','cuser','duser','euser')")]
        public void PipelineDefinitionCanBeBuild(string whereArguments)
        {
            JsonPipelineStageDefinition<BsonDocument, BsonDocument> stage = null;
            Assert.That(() => stage = FilterDefinitionBuilder.Build<BsonDocument>(whereArguments), Throws.Nothing);
            Assert.That(stage, Is.Not.Null);
        }

        [TestCase("Data.Recipient IN ('jdoe','auser')", "{ $match : { \"Data.Recipient\" : { $in : ['jdoe','auser'] } } }")]
        [TestCase("Data.Recipient \t NOT IN  ('jdoe','auser')", "{ $match : { \"Data.Recipient\" : { $nin : ['jdoe','auser'] } } }")]
        [TestCase("Data.Recipient NOT IN ['jdoe','auser']", "{ $match : { \"Data.Recipient\" : { $nin : ['jdoe','auser'] } } }")]
        public void PipelineDefinitionAsExpected(string whereArguments, string expected)
        {
            JsonPipelineStageDefinition<BsonDocument, BsonDocument> stage = null;
            Assert.That(() => stage = FilterDefinitionBuilder.Build<BsonDocument>(whereArguments), Throws.Nothing);
            Assert.That(stage, Is.Not.Null);
            Assert.That(stage.Json, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("Data.IsStandardPlate")]
        [TestCase("Data.Isobars.Exists")]
        [TestCase("Data.FooISNULL")]
        [TestCase("Data.FooLIKE'abc'")]
        [TestCase("Data.FooEXISTS")]
        public void KeywordsInsideFieldNamesAreNotDetected(string whereArguments)
        {
            Assert.That(() => FilterDefinitionBuilder.Build<BsonDocument>(whereArguments), Throws.InstanceOf<FormatException>());
        }

        [Test]
        public void LikeClauseCanHandleRegexCharacters()
        {
            var whereArguments = "Data.BatchId LIKE '%|%'";
            JsonPipelineStageDefinition<BsonDocument, BsonDocument> stage = null;
            Assert.That(() => stage = FilterDefinitionBuilder.Build<BsonDocument>(whereArguments), Throws.Nothing);
            Assert.That(stage.Json.Contains(".*\\\\|.*"));
        }

        [Test]
        public void WildcardsAreReplaced()
        {
            var whereArguments = "Date.BatchId LIKE '%abc%'";
            var stage = FilterDefinitionBuilder.Build<BsonDocument>(whereArguments);
            Assert.That(stage.Json.Contains("%abc%"), Is.False);
        }

        [Test]
        public void InvalidParenthesesCountCausesException()
        {
            var whereArguments = "(timestamp > '2018-01-01' AND (name = 'Test' || Id = 123)"; // Missing final parenthesis
            Assert.That(() => FilterDefinitionBuilder.Build<BsonDocument>(whereArguments), Throws.Exception.TypeOf<FormatException>());
        }
    }
}
