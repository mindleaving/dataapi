namespace SharedViewModels.Helpers
{
    public class NoneMessageBoxSpawner : IMessageBoxSpawner
    {
        public void Show(string message)
        {
            // Do nothing
        }

        public MessageBoxResult Show(string message, string title, MessageBoxButtons buttons)
        {
            return MessageBoxResult.None;
        }
    }
}
