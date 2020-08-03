namespace SharedViewModels.Helpers
{
    public interface IMessageBoxSpawner
    {
        void Show(string message);
        MessageBoxResult Show(string message, string title, MessageBoxButtons buttons);
    }
}