using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using SharedViewModels.Objects;

namespace SharedViewModels.ViewModels
{
    public interface IAutoCompleteTextBoxViewModel : INotifyPropertyChanged
    {
        bool AcceptsNewValues { get; }
        bool IsNewValue { get; }
        string SearchText { get; set; }
        List<ObjectWithDisplayName> SuggestedObjects { get; }
        ObjectWithDisplayName SelectedDisplayNameObject { get; set; }
        bool ShowSuggestions { get; }
        ICommand SelectNextCommand { get; }
        ICommand SelectPreviousCommand { get; }
        ICommand CloseSuggestionsCommand { get; }
        Color BorderColor { get; }
        int BorderThickness { get; }
    }
}