using System.Collections.Generic;
using System.Text.RegularExpressions;
using Commons.Extensions;
using DataAPI.Client.Test.Models;
using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.DataManagement;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataAPI.Client.Test
{
    [TestFixture]
    public class DataApiQueryBuilderTest
    {
        [Test]
        public void CanBuildSelectQuery()
        {
            var query = new DataApiQueryBuilder<TestObject2>()
                .Select(x => x.Id, x => x.EntityId, x => x.Items[-1].ItemId, x => x.Items[2].ItemId)
                .Build();

            var expected = "SELECT Data.Id, Data.EntityId, Data.Items.ItemId, Data.Items.2.ItemId FROM TestObject2";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void CanBuildWhereQuery1()
        {
            var query = new DataApiQueryBuilder<TestObject2>()
                .Where(x => x.Id == "MyID" && (x.EntityId == "P1" || x.ExperimentId == null))
                .Build();

            var expected = "SELECT * FROM TestObject2 WHERE (Data.Id = 'MyID') AND ((Data.EntityId = 'P1') OR (Data.ExperimentId = null))";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void CanBuildWhereQuery2()
        {
            var query = new DataApiQueryBuilder<TestObject3>()
                .Where(x => x.Id == "MyID" && (x.Number1 == 2 || x.Number2 < 3))
                .Build();

            var expected = "SELECT * FROM TestObject3 WHERE (Data.Id = 'MyID') AND ((Data.Number1 = 2) OR (Data.Number2 < 3))";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void BooleanInWhereQueryIsTranslated()
        {
            var query = new DataApiQueryBuilder<DataCollectionProtocol>()
                .Where(x => !x.Parameters[0].IsMandatory)
                .Build();

            var expected = "SELECT * FROM DataCollectionProtocol WHERE Data.Parameters.0.IsMandatory = false";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void CanBuildGroupByQuery()
        {
            var query = new DataApiQueryBuilder<TestObject3>()
                .GroupBy(x => x.Name)
                .Build();

            var expected = "SELECT * FROM TestObject3 GROUP BY Data.Name";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void CanBuildOrderByQuery()
        {
            var query = new DataApiQueryBuilder<TestObject3>()
                .OrderBy(x => x.Number2, SortDirection.Descending)
                .ThenBy(x => x.Number1, SortDirection.Ascending)
                .Build();

            var expected = "SELECT * FROM TestObject3 ORDER BY Data.Number2 DESC, Data.Number1 ASC";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void JsonPropertyAttributesUsed()
        {
            var query = new DataApiQueryBuilder<JsonPropertyTestObject>()
                .Select(x => x.Id, x => x.Age)
                .Where(x => x.Name == "Joe" && x.Age > 50)
                .Build();
            var expected = "SELECT Data._id, Data.age FROM JsonPropertyTestObject WHERE (Data.name = 'Joe') AND (Data.age > 50)";
            Assert.That(query, Is.EqualTo(expected));
        }
        private class JsonPropertyTestObject
        {
            [JsonProperty("_id")]
            public string Id { get; }
            [JsonProperty("name")]
            public string Name { get; }
            [JsonProperty("age")]
            public int Age { get; }
        }

        [Test]
        public void FullQueryTest()
        {
            var query = new DataApiQueryBuilder<TestObject3>()
                .Select(x => x.Id, x => x.Name)
                .Where(x => x.Number1 > 4 && x.Number2 > 4)
                .OrderBy(x => x.Number2, SortDirection.Descending)
                .ThenBy(x => x.Number1, SortDirection.Descending)
                .Limit(5)
                .Build();

            var expected = "SELECT Data.Id, Data.Name FROM TestObject3 WHERE (Data.Number1 > 4) AND (Data.Number2 > 4) ORDER BY Data.Number2 DESC, Data.Number1 DESC LIMIT 5";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void CanHandleNonConstantValuesInQueries()
        {
            var location = new Location("MainSite", "2.31.14");
            var query = new DataApiQueryBuilder<Machine>()
                .Where(x => x.Location.Id == location.Id)
                .Build();
            var expected = "SELECT * FROM Machine WHERE Data.Location.Id = 'MainSite_2.31.14'";
            Assert.That(query, Is.EqualTo(expected));
        }

        [Test]
        public void CanParseExpressionWithMethodCall()
        {
            var builder = new DataApiQueryBuilder<TestObject1>()
                .Where(x => !x.IsDiscontinued && x.SourceSystem == "SelfAssigned");
            string query = null;
            Assert.That(() => query = builder.Build(), Throws.Nothing);
            Assert.That(query, Is.EqualTo("SELECT * FROM TestObject1 WHERE (Data.is_deleted = false) AND (Data.source_system = 'SelfAssigned')"));
        }

        [Test]
        public void CanParseExtensionContainsFilter()
        {
            var sites = new List<string> {"København", "Amsterdam"};
            var builder = new DataApiQueryBuilder<Location>()
                .Where(x => sites.Contains(x.Site));
            string query = null;
            Assert.That(() => query = builder.Build(), Throws.Nothing);
            Assert.That(query, Is.EqualTo("SELECT * FROM Location WHERE Data.Site IN ['København', 'Amsterdam']"));
        }

        [Test]
        public void CanParseIListContainsFilter()
        {
            var sites = new List<string> {"København", "Amsterdam"};
            var builder = new DataApiQueryBuilder<Location>()
                .Where(x => sites.Contains(x.Site));
            string query = null;
            Assert.That(() => query = builder.Build(), Throws.Nothing);
            Assert.That(query, Is.EqualTo("SELECT * FROM Location WHERE Data.Site IN ['København', 'Amsterdam']"));
        }

        [Test]
        public void CanParseInSetFilter()
        {
            var sites = new[] {"København", "Amsterdam"};
            var builder = new DataApiQueryBuilder<Location>()
                .Where(x => x.Site.InSet(sites));
            string query = null;
            Assert.That(() => query = builder.Build(), Throws.Nothing);
            Assert.That(query, Is.EqualTo("SELECT * FROM Location WHERE Data.Site IN ['København', 'Amsterdam']"));
        }

        [Test]
        public void CanParseStartsWithFilter()
        {
            var builder = new DataApiQueryBuilder<Location>()
                .Where(x => x.Site.StartsWith("Arp"));
            string query = null;
            Assert.That(() => query = builder.Build(), Throws.Nothing);
            Assert.That(query, Is.EqualTo("SELECT * FROM Location WHERE Data.Site LIKE 'Arp%'"));
        }

        [Test]
        public void CanParseEndsWithFilter()
        {
            var builder = new DataApiQueryBuilder<Location>()
                .Where(x => x.Site.EndsWith("Arp"));
            string query = null;
            Assert.That(() => query = builder.Build(), Throws.Nothing);
            Assert.That(query, Is.EqualTo("SELECT * FROM Location WHERE Data.Site LIKE '%Arp'"));
        }

        [Test]
        public void CanParseContainsFilter()
        {
            var builder = new DataApiQueryBuilder<Location>()
                .Where(x => x.Site.Contains("Arp"));
            string query = null;
            Assert.That(() => query = builder.Build(), Throws.Nothing);
            Assert.That(query, Is.EqualTo("SELECT * FROM Location WHERE Data.Site LIKE '%Arp%'"));
        }

        [Test]
        public void CanParseRegexIsMatchFilter()
        {
            var builder = new DataApiQueryBuilder<Location>()
                .Where(x => Regex.IsMatch(x.Site, "[a-zA-Z0-9]+"));
            string query = null;
            Assert.That(() => query = builder.Build(), Throws.Nothing);
            Assert.That(query, Is.EqualTo("SELECT * FROM Location WHERE Data.Site LIKE [a-zA-Z0-9]+"));
        }
    }
}
