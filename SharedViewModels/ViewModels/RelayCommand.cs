using System;
using System.Windows.Input;
using SharedViewModels.Helpers;

namespace SharedViewModels.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action action;
        private readonly Func<bool> canExecute;

        public RelayCommand(Action action)
        {
            this.action = action;
            canExecute = () => true;
        }
        public RelayCommand(Action action, Func<bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute();
        }

        public void Execute(object parameter)
        {
            action();
        }

        public event EventHandler CanExecuteChanged
        {
            add { StaticUiUpdateNotifier.Notifier.RequerySuggested += value; }
            remove { StaticUiUpdateNotifier.Notifier.RequerySuggested -= value; }
        }
    }

    public class RelayCommand<T> : ICommand where T : class
    {
        private readonly Action<T> action;
        private readonly Func<T, bool> canExecute;

        public RelayCommand(Action<T> action)
        {
            this.action = action;
            canExecute = _ => true;
        }
        public RelayCommand(Action<T> action, Func<T, bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute(parameter as T);
        }

        public void Execute(object parameter)
        {
            action(parameter as T);
        }

        public event EventHandler CanExecuteChanged
        {
            add { StaticUiUpdateNotifier.Notifier.RequerySuggested += value; }
            remove { StaticUiUpdateNotifier.Notifier.RequerySuggested -= value; }
        }
    }
}