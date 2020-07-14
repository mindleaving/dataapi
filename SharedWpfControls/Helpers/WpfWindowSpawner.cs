using System;
using System.Windows;
using SharedViewModels.Helpers;

namespace SharedWpfControls.Helpers
{
    public class WpfWindowSpawner<T> : IViewSpawner<T>
    {
        private readonly Func<T, Window> windowSpawnFunc;

        public WpfWindowSpawner(Func<T, Window> windowSpawnFunc)
        {
            this.windowSpawnFunc = windowSpawnFunc;
        }

        public bool? SpawnBlocking(T viewModel)
        {
            return windowSpawnFunc(viewModel).ShowDialog();
        }

        public void SpawnNonBlocking(T viewModel)
        {
            windowSpawnFunc(viewModel).Show();
        }
    }
}
