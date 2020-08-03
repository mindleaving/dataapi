using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.Client.Serialization;
using DataAPI.DataStructures;
using Newtonsoft.Json.Linq;
using SharedViewModels.Helpers;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class QueryEditorViewModel : NotifyPropertyChangedBase
    {
        private const int HardLimitCount = 1000;
        private readonly IDataApiClient dataApiClient;

        public QueryEditorViewModel(IDataApiClient dataApiClient, string collectionName)
        {
            this.dataApiClient = dataApiClient;
            
            CollectionName = collectionName;
            RunQueryCommand = new AsyncRelayCommand(RunQuery, CanRunQuery);
        }


        public string CollectionName { get; }
        public string Query { get; set; } = Constants.DefaultQuery;
        private List<JObject> latestSearchResult;
        public List<JObject> LatestSearchResult
        {
            get => latestSearchResult;
            private set
            {
                latestSearchResult = value;
                OnPropertyChanged();
            }
        }

        private bool isSearching;
        public bool IsSearching
        {
            get => isSearching;
            private set
            {
                isSearching = value;
                OnPropertyChanged();
            }
        }

        public IAsyncCommand RunQueryCommand { get; }

        private bool CanRunQuery()
        {
            return !string.IsNullOrWhiteSpace(Query);
        }

        private static bool ContainsFromClause(string query)
        {
            return Regex.IsMatch(query.ToLowerInvariant(), "from\\s+[^\\s]+");
        }

        private bool ContainsLimitClause(string query)
        {
            return Regex.IsMatch(query.ToLowerInvariant(), "limit\\s+[0-9]+");
        }

        private async Task RunQuery()
        {
            if (ContainsFromClause(Query))
            {
                StaticMessageBoxSpawner.Show("Query must not contain a FROM clause");
                return;
            }

            if (!ContainsLimitClause(Query))
            {
                var confirmResult = StaticMessageBoxSpawner.Show(
                    "Your query doesn't have a LIMIT clause, which might result in enormous amounts of results, "
                    + "that the application may not be able to handle. "
                    + "Are you sure you want to run this query?",
                    "Confirm query without limit",
                    MessageBoxButtons.YesNo);
                if(confirmResult != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                IsSearching = true;
                var completedQuery = $"{Query} FROM {CollectionName}";
                //if (!completedQuery.ToUpperInvariant().Contains("LIMIT"))
                //    completedQuery += $" LIMIT {HardLimitCount}";
                var resultStream = await dataApiClient.SearchAsync(
                    completedQuery,
                    ResultFormat.Json);
                var parsedJson = await resultStream.ReadAllSearchResultsAsync();
                LatestSearchResult = parsedJson;
                JsonSearchFinished?.Invoke(this, parsedJson);
            }
            catch (Exception e)
            {
                var innermostException = e.InnermostException();
                StaticMessageBoxSpawner.Show($"Error: {innermostException.Message}");
            }
            finally
            {
                IsSearching = false;
            }
        }

        public event EventHandler<List<JObject>> JsonSearchFinished;
    }
}