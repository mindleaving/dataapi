using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.DataStructures;
using DataAPI.DataStructures.UserManagement;
using DataAPI.IntegrationTest.DataObjects;
using NUnit.Framework;

namespace DataAPI.IntegrationTest
{
    [TestFixture]
    [Category("Performance")]
    [Ignore("Tool")]
    public class PerformanceTest : ApiTestBase
    {
        [Test]
        public async Task MeasureOutputPerformance()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var dataApiClient);

            var resultFormat = ResultFormat.Json;
            var resultStream = await dataApiClient.SearchAsync($"SELECT Data.id FROM {DataApiClient.GetCollectionName<UnitTestDataObject1>()}", resultFormat);
            var resultTable = await Client.Serialization.SeachResultStreamExtensions.ReadAllSearchResultsAsync(resultStream);
            var stopWatch = Stopwatch.StartNew();
            var batches = new List<UnitTestDataObject1>();
            foreach (var componentId in resultTable.Select(x => x.Value<string>("Data_id")))
            {
                var batch = dataApiClient.GetAsync<UnitTestDataObject1>(componentId).Result;
                batches.Add(batch);
            }
            stopWatch.Stop();
            var rowCount = batches.Count;
            var averageRetreivalTime = stopWatch.Elapsed.TotalMilliseconds / rowCount;
            Console.WriteLine($"It took {stopWatch.Elapsed.TotalSeconds:F3}s to retreive {rowCount} objects (avg: {averageRetreivalTime:F0}ms)");

            UserGenerator.DeleteUser(dataApiClient);
        }

        [Test]
        public void MeasureInputPerformance()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var dataApiClient);

            var testObjects = Enumerable.Range(0, 1000).Select(idx => new UnitTestDataObject1()).ToList();
            var stopWatch = Stopwatch.StartNew();
            foreach (var testObject in testObjects)
            {
                dataApiClient.InsertAsync(testObject).Wait();
            }
            stopWatch.Stop();
            var averageStorageTime = stopWatch.Elapsed.TotalMilliseconds / testObjects.Count;
            Console.WriteLine($"It took {stopWatch.Elapsed.TotalSeconds:F3}s to store {testObjects.Count} objects (avg: {averageStorageTime:F0}ms)");
            foreach (var testObject in testObjects)
            {
                dataApiClient.DeleteAsync<UnitTestDataObject1>(testObject.Id).Wait();
            }

            UserGenerator.DeleteUser(dataApiClient);
        }
    }
}
