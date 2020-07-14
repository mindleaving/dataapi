using System;

namespace SharedViewModels.Helpers
{
    public class CollectionSwitcher
        : ICollectionSwitcher
    {
        private readonly Action<string, string> callback;
        private readonly string defaultQuery;

        public CollectionSwitcher(Action<string, string> callback, string defaultQuery)
        {
            this.callback = callback;
            this.defaultQuery = defaultQuery;
        }

        public void SwitchTo(string collectionName, string query = null)
        {
            if (query == null)
                query = defaultQuery;
            callback(collectionName, query);
        }
    }
}
