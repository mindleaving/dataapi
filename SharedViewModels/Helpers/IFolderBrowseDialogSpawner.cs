namespace SharedViewModels.Helpers
{
    public interface IFolderBrowseDialogSpawner
    {
        IFolderBrowseDialog Spawn(string title, bool showNewFolderButton);
    }

    public interface IFolderBrowseDialog
    {
        string SelectedPath { get; }
        bool? ShowDialog();
    }
}