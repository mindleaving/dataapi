using System.Threading.Tasks;
using System.Windows.Input;

namespace SharedViewModels.ViewModels
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}
