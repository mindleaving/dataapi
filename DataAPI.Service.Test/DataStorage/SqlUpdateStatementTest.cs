using System;
using DataAPI.Service.DataStorage;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataAPI.Service.Test.DataStorage
{
    [TestFixture]
    public class SqlUpdateStatementTest
    {
        [Test]
        public void StatementFromContainerAsExpected()
        {
            var dataType = "SqlUnitTestObject1";
            var createdTimeUtc = DateTime.Parse("2019-08-27 01:02:03");
            var container = new GenericDataContainer(
                "1234",
                "jdoe",
                createdTimeUtc, 
                "jdoe",
                createdTimeUtc, 
                ApiVersion.Current,
                DataEncoder.Encode(JsonConvert.SerializeObject(new {Name = "Doe", Age = 101.4, TimeUtc = createdTimeUtc})));
            var actual = SqlUpdateStatement.CreateFromContainer(dataType, container);
            var expected = $"UPDATE {dataType} SET Submitter='jdoe', SubmissionTimeUtc='2019-08-27 01:02:03', Data#Name='Doe', Data#Age='101.4', Data#TimeUtc='2019-08-27 01:02:03' WHERE Id='1234'";
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}