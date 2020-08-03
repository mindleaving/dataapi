using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Client.Repositories;
using DataAPI.Client.Test.Models;
using DataAPI.DataStructures;
using Moq;
using NUnit.Framework;

namespace DataAPI.Client.Test
{
    [TestFixture]
    public class ExpressionParserTest
    {
        [Test]
        public void ConditionalExpressionParsedCorrectly()
        {
            var dataApiClient = new Mock<IDataApiClient>();
            string actualQuery = null;
            dataApiClient
                .Setup(x => x.SearchAsync(It.IsAny<string>(), ResultFormat.Json))
                .Callback<string, ResultFormat>((query, format) => actualQuery = query)
                .Returns(Task.FromResult(Stream.Null));
            var sut = new GenericDatabase<TestObject1>(dataApiClient.Object);

            // Test 1
            var sourceSystem = "SAP";
            sut.Where(x => sourceSystem != null ? x.SourceSystem == sourceSystem : x.SourceSystem != "MDS").ToList();

            Assert.That(actualQuery, Is.EqualTo("FROM TestObject1 WHERE Data.source_system = 'SAP'"));

            // Test 1
            sourceSystem = null;
            sut.Where(x => sourceSystem != null ? x.SourceSystem == sourceSystem : x.SourceSystem != "MDS").ToList();

            Assert.That(actualQuery, Is.EqualTo("FROM TestObject1 WHERE Data.source_system != 'MDS'"));
        }
    }
}
