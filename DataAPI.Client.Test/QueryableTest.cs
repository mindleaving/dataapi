using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAPI.Client.Repositories;
using DataAPI.Client.Test.Models;
using DataAPI.DataStructures;
using Moq;
using NinjaNye.SearchExtensions;
using NUnit.Framework;

namespace DataAPI.Client.Test
{
    [TestFixture]
    public class QueryableTest
    {
        [Test]
        public void SupportsNinjaNyeSearch()
        {
            var searchText = "arla hp";
            var searchTerms = searchText.Split();
            var resultStream = new MemoryStream(Encoding.UTF8.GetBytes(
                    "{\"Data\": {  \"id\": \"1179240\",  \"source_id\": \"84\",  \"business_name\": \"Letmælk\",  \"source_system\": \"SQL\",  \"source_table\": \"ingredients_dbo_Ingredient\",  \"component_type\": \"Ingredient\",  \"created_on\": \"1/13/2017 9:58:45 AM\",  \"created_by\": \"JDOE\",  \"updated_on\": null,  \"updated_by\": null,  \"is_deleted\": \"False\"} }"
            ));

            var dataApiClient = new Mock<IDataApiClient>();
            dataApiClient.Setup(x => x.SearchAsync(It.IsAny<string>(), ResultFormat.Json))
                .Returns(Task.FromResult((Stream)resultStream));
            var queryable = new GenericDatabase<TestObject1>(dataApiClient.Object);
            var filter = "MDS";
            var sourceSystem = filter != "all" ? filter : null;
            var searchResult = queryable
                .Where(x => sourceSystem != null ? x.SourceSystem == filter : x.SourceSystem != "MDS")
                .Search(x => x.Name, x => x.SourceId)
                .ContainingAll(searchTerms)
                .OrderBy(x => x.Name)
                .ThenByDescending(x => x.CreatedBy)
                .Select(x => x.Id)
                .ToList();
            Assert.That(searchResult.Count, Is.EqualTo(1));
            Assert.That(searchResult[0], Is.EqualTo("1179240"));
        }

        [Test]
        [NUnit.Framework.Category("IntegrationTest")]
        public void QueryableIntegrationTest()
        {
            var ids = new List<string> {"1228357", "1228358"};
            var dataApiClient = new DataApiClient(new ApiConfiguration("", 443));
            var queryable = new GenericDatabase<TestObject1>(dataApiClient);
            var searchResult = queryable.Where(x => ids.Contains(x.Id)).ToList();
            Assert.That(searchResult, Is.Not.Null);
            CollectionAssert.AreEqual(ids, searchResult.Select(x => x.Id));
        }

        [Test]
        public void AutocompleteWithGenericDatabaseQueryable()
        {
            var resultStream = new MemoryStream(Encoding.UTF8.GetBytes(
                "{\"Data\": {  \"id\": \"1179240\",  \"source_id\": \"84\",  \"business_name\": \"Letmælk\",  \"source_system\": \"SQL\",  \"source_table\": \"ingredients_dbo_Ingredient\",  \"component_type\": \"Ingredient\",  \"created_on\": \"1/13/2017 9:58:45 AM\",  \"created_by\": \"JDOE\",  \"updated_on\": null,  \"updated_by\": null,  \"is_deleted\": \"False\"} }"
            ));

            var dataApiClient = new Mock<IDataApiClient>();
            dataApiClient
                .Setup(x => x.SearchAsync(It.IsAny<string>(), ResultFormat.Json))
                .Returns(Task.FromResult((Stream)resultStream));
            var trialRepository = new GenericDatabase<TestObject1>(dataApiClient.Object);
            Assert.That(() => trialRepository.Where(x => x.Id.StartsWith("abc")).Take(10).ToList(), Throws.Nothing);
        }
    }
}
