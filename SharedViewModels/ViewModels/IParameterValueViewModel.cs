using System.ComponentModel;

namespace SharedViewModels.ViewModels
{
    public interface IParameterValueViewModel : INotifyPropertyChanged
    {
        string Value { get; set; }
    }
}
