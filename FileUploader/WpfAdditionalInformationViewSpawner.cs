using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileHandlers.AdditionalInformation;
using FileUploader.ViewModel;
using FileUploader.Views;

namespace FileUploader
{
    public class WpfAdditionalInformationViewSpawner : IAdditionalInformationViewSpawner
    {
        public Task<bool?> Spawn(string filename, List<IAdditionalInformationViewModel> viewModels)
        {
            var viewModel = new AdditionalInformationViewModel(filename, viewModels);
            var view = new AdditionalInformationWindow
            {
                ViewModel = viewModel
            };

            var taskCompletion = new TaskCompletionSource<bool?>();
            view.Dispatcher.BeginInvoke(new Action(() => taskCompletion.SetResult(view.ShowDialog())));

            return taskCompletion.Task;
        }
    }
}
