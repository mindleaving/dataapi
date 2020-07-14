using SharedViewModels.Helpers;
using SharedViewModels.Objects;
using SharedWpfControls.Objects;

namespace SharedWpfControls.Helpers
{
    public class WpfPasswordBoxSpawner : IPasswordBoxSpawner
    {
        public IPasswordBox SpawnNew(int tabIndex = 1)
        {
            return new WpfPasswordBox(tabIndex);
        }
    }
}
