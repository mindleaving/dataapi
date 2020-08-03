using AutoCompleteMatchers;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;

namespace SharedViewModels.ViewModels
{
    public class ProjectSelectionViewModel : NotifyPropertyChangedBase
    {
        public ProjectSelectionViewModel(IReadonlyObjectDatabase<DataProject> projectDatabase)
        {
            ProjectAutoCompleteViewModel = new AutoCompleteTextBoxViewModel<DataProject>(
                x => x.Id,
                projectDatabase,
                new DataProjectAutoCompleteMatcher());
        }

        public AutoCompleteTextBoxViewModel<DataProject> ProjectAutoCompleteViewModel { get; }
    }
}
