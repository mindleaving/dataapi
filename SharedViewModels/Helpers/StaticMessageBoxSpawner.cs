namespace SharedViewModels.Helpers
{
    public static class StaticMessageBoxSpawner
    {
        public static IMessageBoxSpawner Spawner { get; set; }

        public static void Show(string message) 
            => Spawner.Show(message);

        public static MessageBoxResult Show(string message, string title, MessageBoxButtons buttons)
            => Spawner.Show(message, title, buttons);
    }
}
