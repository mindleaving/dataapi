using System;

namespace SharedViewModels.Helpers
{
    public static class StaticUiUpdateNotifier
    {
        public static IUiUpdateNotifier Notifier { get; set; }
    }

    public interface IUiUpdateNotifier
    {
        event EventHandler RequerySuggested;
    }
}
