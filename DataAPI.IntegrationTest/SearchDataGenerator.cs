using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commons;
using DataAPI.Client;
using DataAPI.IntegrationTest.DataObjects;

namespace DataAPI.IntegrationTest
{
    public class SearchDataGenerator
    {
        public static List<UnitTestSearchObject> GenerateAndSubmitSearchData(int searchObjectCount, IDataApiClient dataApiClient)
        {
            var searchObjects = new List<UnitTestSearchObject>();
            var insertionTasks = new List<Task>();
            for (int objectIdx = 0; objectIdx < searchObjectCount; objectIdx++)
            {
                var searchObject = GenerateUnitTestSearchObject();
                insertionTasks.Add(dataApiClient.InsertAsync(searchObject, searchObject.Id));
                searchObjects.Add(searchObject);
            }
            Task.WaitAll(insertionTasks.ToArray());
            return searchObjects;
        }

        public static UnitTestSearchObject GenerateUnitTestSearchObject()
        {
            var categories = Enumerable.Range(0, 3).Select(_ => Guid.NewGuid().ToString()).ToList();
            var searchObject = new UnitTestSearchObject
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Categories = categories,
                Products = Enumerable.Range(0, 2).Select(
                    x1 => new UnitTestSearchObject.Product
                    {
                        Title = Guid.NewGuid().ToString(),
                        Price = 100 * StaticRandom.Rng.NextDouble(),
                        Categories = Enumerable.Range(0, 2).Select(x2 => categories[StaticRandom.Rng.Next(categories.Count)]).ToList()
                    }).ToList()
            };
            return searchObject;
        }

        public static void DeleteData(List<UnitTestSearchObject> searchData, IDataApiClient dataApiClient)
        {
            DeleteData(searchData.Select(x => x.Id), dataApiClient);
        }
        public static void DeleteData(IEnumerable<string> ids, IDataApiClient dataApiClient)
        {
            var tasks = new List<Task>();
            foreach (var id in ids)
            {
                var task = dataApiClient.DeleteAsync<UnitTestSearchObject>(id);
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}
