using Ookii.Dialogs.Wpf;
using SharedViewModels.Helpers;

namespace SharedWpfControls.Helpers
{
    public class WpfFolderBrowseDialog : IFolderBrowseDialog
    {
        public WpfFolderBrowseDialog(string title, bool showNewFolderButton)
        {
            BackingDialog = new VistaFolderBrowserDialog
            {
                Description = title,
                UseDescriptionForTitle = true,
                ShowNewFolderButton = showNewFolderButton
            };
        }

        private VistaFolderBrowserDialog BackingDialog { get; }
        public string SelectedPath => BackingDialog.SelectedPath;

        public bool? ShowDialog()
        {
            return BackingDialog.ShowDialog();
        }
    }
}
