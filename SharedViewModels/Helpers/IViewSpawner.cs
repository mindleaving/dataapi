namespace SharedViewModels.Helpers
{
    public interface IViewSpawner<in T>
    {
        bool? SpawnBlocking(T viewModel);
        void SpawnNonBlocking(T viewModel);
    }
}
