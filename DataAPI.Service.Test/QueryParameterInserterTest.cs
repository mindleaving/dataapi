using System.Collections.Generic;
using DataAPI.Service.Search;
using NUnit.Framework;

namespace DataAPI.Service.Test
{
    [TestFixture]
    public class QueryParameterInserterTest
    {
        [Test]
        public void ThrowsArgumentNullIfQueryNull()
        {
            string query = null;
            var parameters = new Dictionary<string, List<string>>
            {
                {"par1", new List<string> {"abc"}},
                {"par2", new List<string> {"edf"}},
                {"notused", new List<string> {"0"}}
            };

            Assert.That(() => QueryParameterInserter.InsertParameters(query, parameters), Throws.ArgumentNullException);
        }

        [Test]
        public void ThrowsArgumentNullIfParameterDictionaryNull()
        {
            var query = "SELECT {par1} FROM {par2} WHERE id = '{Par_3}'";

            Assert.That(() => QueryParameterInserter.InsertParameters(query, null), Throws.ArgumentNullException);
        }

        [Test]
        public void ThrowsIfAnyParameterMissing()
        {
            var query = "SELECT {par1} FROM {par2} WHERE id = '{Par_3}'";
            var parameters = new Dictionary<string, List<string>>
            {
                {"par1", new List<string> {"abc"}},
                {"par2", new List<string> {"edf"}},
                {"notused", new List<string> {"0"}}
            };

            Assert.That(() => QueryParameterInserter.InsertParameters(query, parameters), Throws.Exception);
        }

        [Test]
        public void ThrowsIfAnyParameterHasMultipleValues()
        {
            var query = "SELECT {par1} FROM {par2} WHERE id = '{Par_3}'";
            var parameters = new Dictionary<string, List<string>>
            {
                {"par1", new List<string> {"abc"}},
                {"par2", new List<string> {"edf", "ghi"}},
                {"Par_3", new List<string> {"0"}}
            };

            Assert.That(() => QueryParameterInserter.InsertParameters(query, parameters), Throws.Exception);
        }

        [Test]
        public void ThrowsIfAnyParameterContainsCurlyBrackets()
        {
            var query = "SELECT {par1} FROM {par2} WHERE id = '{Par_3}'";
            var parameters = new Dictionary<string, List<string>>
            {
                {"par1", new List<string> {"abc}"}},
                {"par2", new List<string> {"{edf"}},
                {"Par_3", new List<string> {"0"}}
            };

            Assert.That(() => QueryParameterInserter.InsertParameters(query, parameters), Throws.Exception);
        }

        [Test]
        public void ParametersAreInsertedInQuery()
        {
            var query = "SELECT {par1} FROM {par2} WHERE id = '{Par_3}'";
            var parameters = new Dictionary<string, List<string>>
            {
                {"par1", new List<string> {"abc"}},
                {"par2", new List<string> {"edf"}},
                {"Par_3", new List<string> {"0"}}
            };

            string actual = null;
            Assert.That(() => actual = QueryParameterInserter.InsertParameters(query, parameters), Throws.Nothing);
            Assert.That(actual, Is.EqualTo("SELECT abc FROM edf WHERE id = '0'"));
        }
    }
}
