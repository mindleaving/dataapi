namespace SharedViewModels.Helpers
{
    public interface ICollectionSwitcher
    {
        void SwitchTo(string collectionName, string query = null);
    }
}