using SharedViewModels.Helpers;
using SharedWpfControls.Views;

namespace SharedWpfControls.Helpers
{
    public class WpfFolderBrowseDialogSpawner : IFolderBrowseDialogSpawner
    {
        public IFolderBrowseDialog Spawn(string title, bool showNewFolderButton)
        {
            return new WpfFolderBrowseDialog(title, showNewFolderButton);
        }
    }
}
