using System;
using System.Windows.Input;
using SharedViewModels.Helpers;

namespace SharedWpfControls.Helpers
{
    public class WpfUiUpdateNotifier : IUiUpdateNotifier
    {
        public event EventHandler RequerySuggested
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
