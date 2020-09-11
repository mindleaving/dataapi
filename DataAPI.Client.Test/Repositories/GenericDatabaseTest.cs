using System.Collections.Generic;
using System.Threading.Tasks;
using DataAPI.Client.Repositories;
using DataAPI.Client.Test.Models;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DataAPI.Client.Test.Repositories
{
    [TestFixture]
    public class GenericDatabaseTest
    {
        private readonly Mock<IDataApiClient> dataApiClient;

        public GenericDatabaseTest()
        {
            dataApiClient = new Mock<IDataApiClient>();
            dataApiClient.Setup(x => x.IsLoggedIn).Returns(true);
        }

        [Test]
        public async Task GetManyStringWhereClauseIsUsed()
        {
            const string WhereClause = "Data.source_id LIKE 'abc%'";
            string actual = null;
            dataApiClient.Setup(x => x.GetManyAsync<TestObject1>(It.IsAny<string>(), null, null))
                .Callback<string, string, uint?>((whereArguments, orderByArguments, limit) => actual = whereArguments)
                .ReturnsAsync(new List<TestObject1>());
            var sut = new GenericDatabase<TestObject1>(dataApiClient.Object);

            await sut.GetManyAsync(WhereClause);

            Assert.That(actual, Is.EqualTo(WhereClause));
        }

        [Test]
        public async Task GetManyFilterExpressionIsUsed()
        {
            const string Expected = "Data.source_id LIKE 'abc%'";
            string actual = null;
            dataApiClient.Setup(x => x.GetManyAsync<TestObject1>(It.IsAny<string>(), null, null))
                .Callback<string, string, uint?>((whereArguments, orderByArguments, limit) => actual = whereArguments)
                .ReturnsAsync(new List<TestObject1>());
            var sut = new GenericDatabase<TestObject1>(dataApiClient.Object);

            await sut.GetManyAsync(x => x.SourceId.StartsWith("abc"));

            Assert.That(actual, Is.EqualTo(Expected));
        }

        [Test]
        public async Task PermanentFilterIsAppliedForTypedRepository()
        {
            string actual = null;
            dataApiClient.Setup(x => x.GetManyAsync<TestObject1>(It.IsAny<string>(), null, null))
                .Callback<string, string, uint?>((whereArguments, orderByArguments, limit) => actual = whereArguments)
                .ReturnsAsync(new List<TestObject1>());
            var sut = new GenericDatabase<TestObject1>(dataApiClient.Object, x => x.SourceSystem == "XY");

            await sut.GetManyAsync("Data.source_id LIKE 'abc%'");

            Assert.That(actual, Is.EqualTo("Data.source_system = 'XY' AND (Data.source_id LIKE 'abc%')"));
        }

        [Test]
        public async Task PermanentFilterIsAppliedForTypelessRepository()
        {
            string actual = null;
            dataApiClient.Setup(x => x.GetManyAsync("TestObject1", It.IsAny<string>(), null, null))
                .Callback<string, string, string, uint?>((dataType, whereArguments, orderByArguments, limit) => actual = whereArguments)
                .ReturnsAsync(new List<string>());
            var sut = new GenericDatabase(dataApiClient.Object, "TestObject1", "Data.source_system = 'XY'");

            await sut.GetManyAsync("Data.source_id LIKE 'abc%'");

            Assert.That(actual, Is.EqualTo("Data.source_system = 'XY' AND (Data.source_id LIKE 'abc%')"));
        }
    }
}
