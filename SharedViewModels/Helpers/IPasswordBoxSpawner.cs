using SharedViewModels.Objects;

namespace SharedViewModels.Helpers
{
    public interface IPasswordBoxSpawner
    {
        IPasswordBox SpawnNew(int tabIndex = 1);
    }
}