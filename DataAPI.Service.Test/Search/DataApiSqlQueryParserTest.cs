using System;
using DataAPI.Service.Objects;
using DataAPI.Service.Search;
using NUnit.Framework;

namespace DataAPI.Service.Test.Search
{
    [TestFixture]
    public class DataApiSqlQueryParserTest
    {
        [Test]
        public void DuplicateFromKeywordThrowsException()
        {
            var query = "SELECT * FROM Component FROM Plate WHERE _id = '3'";
            Assert.That(() => DataApiSqlQueryParser.Parse(query), Throws.TypeOf<FormatException>());
        }

        [Test]
        public void DuplicateWhereKeywordIsParsed()
        {
            var query = "SELECT * FROM Component WHERE Data.business_name LIKE '%arla%' WHERE Data.source_id = '3'";
            DataApiSqlQuery actual = null;
            Assert.That(() => actual = DataApiSqlQueryParser.Parse(query), Throws.Nothing);
            Assert.That(actual.WhereArguments, Is.EqualTo("(Data.business_name LIKE '%arla%') AND (Data.source_id = '3')"));
        }

        [Test]
        public void SqlLikeQueryHandlesDifferentWhitespaces()
        {
            var query = "SELECT Product.Name From Products where timestamp >= '2018-01-01'      AND Product.Price.Netto < 300\n" +
                        "ORDER BY timestamp ASC SKIP 5 LIMIT 10";

            DataApiSqlQuery actual = null;
            Assert.That(() => actual = DataApiSqlQueryParser.Parse(query), Throws.Nothing);
            Assert.That(actual.SelectArguments, Is.EqualTo("Product.Name"));
            Assert.That(actual.FromArguments, Is.EqualTo("Products"));
            Assert.That(actual.WhereArguments, Is.EqualTo("timestamp >= '2018-01-01' AND Product.Price.Netto < 300"));
            Assert.That(actual.OrderByArguments, Is.EqualTo("timestamp ASC"));
            Assert.That(actual.SkipArguments, Is.EqualTo("5"));
            Assert.That(actual.LimitArguments, Is.EqualTo("10"));
            Assert.That(actual.GroupByArguments, Is.Null);
            Assert.That(actual.JoinArguments, Is.Null);
        }

        [Test]
        [TestCase("SELECT * FROM MyCollection WHERE Data.NonExistingProperty IS NULL")]
        [TestCase("SELECT * FROM MyCollection WHERE Data.ExistingProperty IS NOT NULL")]
        [TestCase("SELECT * FROM MyCollection WHERE Data.ExistingProperty EXISTS")]
        [TestCase("SELECT * FROM MyCollection WHERE Data.NonExistingProperty NOT EXISTS")]
        [TestCase("SELECT * FROM MyCollection SORT BY Data.Number")]
        public void SqlLikeQueryIsUnderstood(string query)
        {
            Assert.That(() => DataApiSqlQueryParser.Parse(query), Throws.Nothing);
        }
    }
}
