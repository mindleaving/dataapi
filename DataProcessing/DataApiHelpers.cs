using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.Client.Serialization;
using DataAPI.DataStructures;

namespace DataProcessing
{
    public static class DataApiHelpers
    {
        public static async Task<BatchUploadResult> BatchUpload(
            IEnumerable<IId> objects, 
            IDataApiClient dataApiClient,
            bool overwriteIfExists = false)
        {
            const int MaxSimultanuousUploads = 10;

            var uploadTasks = new List<Task>();
            var objectsCount = 0;
            var successfulUploadCount = 0;
            var failedUploadCount = 0;
            var modifiedCount = 0;
            using(var enumerator = objects.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    objectsCount++;
                    var uploadTask = Upload(enumerator.Current, dataApiClient, overwriteIfExists);
                    uploadTasks.Add(uploadTask);
                    if (uploadTasks.Count < MaxSimultanuousUploads) 
                        continue;
                    try
                    {
                        await Task.WhenAll(uploadTasks.ToArray());
                    }
                    catch (AggregateException) { }
                    catch (TaskCanceledException) { }
                    var failedTasks = uploadTasks.Where(task => task.Status.InSet(TaskStatus.Canceled, TaskStatus.Faulted)).ToList();
                    failedUploadCount += failedTasks.Count;
                    failedTasks.ForEach(task => uploadTasks.Remove(task));
                    var successfulTasks = uploadTasks.Where(task => task.Status == TaskStatus.RanToCompletion).ToList();
                    successfulUploadCount += successfulTasks.Count;
                    successfulTasks.ForEach(task => uploadTasks.Remove(task));
                    modifiedCount += successfulTasks.Cast<Task<bool>>().Count(task => task.Result == true);
                }
            }
            await Task.WhenAll(uploadTasks.ToArray());
            failedUploadCount += uploadTasks.Count(task => task.Status.InSet(TaskStatus.Canceled, TaskStatus.Faulted));
            successfulUploadCount += uploadTasks.Count(task => task.Status == TaskStatus.RanToCompletion);
            modifiedCount += uploadTasks
                .Where(task => task.Status == TaskStatus.RanToCompletion)
                .Cast<Task<bool>>()
                .Count(task => task.Result == true);

            return new BatchUploadResult(objectsCount, successfulUploadCount, failedUploadCount, modifiedCount);
        }

        /// <summary>
        /// Inserts object into database if it doesn't already exist
        /// </summary>
        /// <returns>True if the object was inserted, otherwise false</returns>
        public static async Task<bool> Upload(
            IId obj, 
            IDataApiClient dataApiClient,
            bool overwriteIfExists = false)
        {
            if (overwriteIfExists)
            {
                await dataApiClient.ReplaceAsync(obj, obj.Id);
                return true;
            }
            var exists = await dataApiClient.ExistsAsync(obj.GetType().Name, obj.Id);
            if (!exists)
            {
                await dataApiClient.InsertAsync(obj, obj.Id);
            }
            return !exists;
        }

        public static async Task<DateTime?> GetLatestTrialTimestamp(IDataApiClient dataApiClient, string trialIdPattern)
        {
            var trialTimestampQuery = "SELECT Data.CreatedTimestamp AS CreatedTimestamp FROM Trial";
            if(trialIdPattern != null)
                trialTimestampQuery += $" WHERE _id LIKE '{trialIdPattern}'";
            trialTimestampQuery += " ORDER BY Data.CreatedTimestamp DESC LIMIT 1";
            var resultStream = await dataApiClient.SearchAsync(trialTimestampQuery, ResultFormat.Json);
            var searchResults = await resultStream.ReadAllSearchResultsAsync();

            if (!searchResults.Any())
                return null;
            return searchResults.First().Value<DateTime>("CreatedTimestamp");
        }
    }
}
