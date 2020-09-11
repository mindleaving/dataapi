using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.Client.Repositories;
using DataAPI.Client.Test.Models;
using DataAPI.DataStructures;
using DataAPI.DataStructures.UserManagement;
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

        [Test]
        public void EnumParsedToString()
        {
            var dataApiClient = new Mock<IDataApiClient>();
            string actualQuery = null;
            dataApiClient
                .Setup(x => x.SearchAsync(It.IsAny<string>(), ResultFormat.Json))
                .Callback<string, ResultFormat>((query, format) => actualQuery = query)
                .Returns(Task.FromResult(Stream.Null));
            var sut = new GenericDatabase<ClassWithEnum>(dataApiClient.Object);

            // Test 1
            sut.Where(x => x.Role == Role.Analyst).ToList();
            Assert.That(actualQuery, Is.EqualTo("FROM ClassWithEnum WHERE Data.Role = 'Analyst'"));

            // Test 2
            sut.Where(x => x.Role.InSet(Role.Viewer, Role.DataProducer)).ToList();
            Assert.That(actualQuery, Is.EqualTo("FROM ClassWithEnum WHERE Data.Role IN ['Viewer', 'DataProducer']"));

            // Test 3
            var test3Roles = new[] {Role.UserManager, Role.Analyst};
            sut.Where(x => test3Roles.Contains(x.Role)).ToList();
            Assert.That(actualQuery, Is.EqualTo("FROM ClassWithEnum WHERE Data.Role IN ['UserManager', 'Analyst']"));
        }

        private class ClassWithEnum : IId
        {
            public ClassWithEnum(Role role)
            {
                Id = Guid.NewGuid().ToString();
                Role = role;
            }

            public string Id { get; }
            public Role Role { get; }
        }
    }
}
