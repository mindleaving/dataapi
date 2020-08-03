using System;
using DataAPI.Service.DataStorage;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataAPI.Service.Test.DataStorage
{
    [TestFixture]
    public class SqlInsertStatementTest
    {
        [Test]
        public void StatementAsExpected()
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
                DataEncoder.Encode(JsonConvert.SerializeObject(new {Name = "Doe", Age = 101.4, TimeUtc = createdTimeUtc })));
            var actual = SqlInsertStatement.CreateFromContainer(dataType, container);
            var expected = $"INSERT INTO {dataType} (Id, OriginalSubmitter, CreatedTimeUtc, Submitter, SubmissionTimeUtc, Data#Name, Data#Age, Data#TimeUtc) "
                           + "VALUES ('1234', 'jdoe', '2019-08-27 01:02:03', 'jdoe', '2019-08-27 01:02:03', 'Doe', '101.4', '2019-08-27 01:02:03')";
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
