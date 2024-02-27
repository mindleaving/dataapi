using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataAPI.Service.Helpers;
using DataAPI.Service.Search;
using NUnit.Framework;

namespace DataAPI.Service.Test.Search
{
    [TestFixture]
    public class SearchResultStreamTest
    {
        [Test]
        public void StreamWithoutItemsDoesntThrowExceptions()
        {
            var asyncEnumerable = AsyncEnumerableBuilder.FromArray(new string[0]);
            using var sut = new SearchResultStream(asyncEnumerable);
            using var streamReader = new StreamReader(sut);
            var actual = ReadAllLines(streamReader).ToList();
            Assert.That(actual, Is.Empty);
        }

        [Test]
        public void StreamReturnsInputItems()
        {
            var expected = new[] {"Item1", "Item2", "Item3"};
            var asyncEnumerable = AsyncEnumerableBuilder.FromArray(expected);
            using var sut = new SearchResultStream(asyncEnumerable);
            using var streamReader = new StreamReader(sut);
            var actual = ReadAllLines(streamReader).ToList();
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IEnumerable<string> ReadAllLines(TextReader streamReader)
        {
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}
