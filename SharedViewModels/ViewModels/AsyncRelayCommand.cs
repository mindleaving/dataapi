using System;
using System.Threading.Tasks;
using SharedViewModels.Helpers;

namespace SharedViewModels.ViewModels
{
    public class AsyncRelayCommand : IAsyncCommand
    {
        private readonly Func<Task> action;
        private readonly Func<bool> canExecute;
        private volatile bool isExecuting;

        public AsyncRelayCommand(Func<Task> action)
        {
            this.action = action;
            canExecute = () => true;
        }
        public AsyncRelayCommand(Func<Task> action, Func<bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return !isExecuting && canExecute();
        }

        public async void Execute(object parameter)
        {
            isExecuting = true;
            try
            {
                await action();
            }
            finally
            {
                isExecuting = false;
            }
        }

        public async Task ExecuteAsync(object parameter)
        {
            await action();
        }

        public event EventHandler CanExecuteChanged
        {
            add { StaticUiUpdateNotifier.Notifier.RequerySuggested += value; }
            remove { StaticUiUpdateNotifier.Notifier.RequerySuggested -= value; }
        }
    }

    public class AsyncRelayCommand<T> : IAsyncCommand
    {
        private readonly Func<T, Task> action;
        private readonly Func<T, bool> canExecute;
        private volatile bool isExecuting;

        public AsyncRelayCommand(Func<T, Task> action)
        {
            this.action = action;
            canExecute = obj => true;
        }
        public AsyncRelayCommand(Func<T, Task> action, Func<T, bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return !isExecuting && canExecute((T)parameter);
        }

        public async void Execute(object parameter)
        {
            isExecuting = true;
            try
            {
                await action((T)parameter);
            }
            finally
            {
                isExecuting = false;
            }
        }

        public async Task ExecuteAsync(object parameter)
        {
            await action((T) parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { StaticUiUpdateNotifier.Notifier.RequerySuggested += value; }
            remove { StaticUiUpdateNotifier.Notifier.RequerySuggested -= value; }
        }
    }
}