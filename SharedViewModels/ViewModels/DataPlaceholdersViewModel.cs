using DataAPI.DataStructures.DataManagement;

namespace SharedViewModels.ViewModels
{
    public class DataPlaceholdersViewModel : NotifyPropertyChangedBase
    {
        public DataPlaceholdersViewModel(DataPlaceholder dataPlaceholder = null)
        {
            if (dataPlaceholder != null)
            {
                DataType = dataPlaceholder.DataType;
                Name = dataPlaceholder.Name;
                Description = dataPlaceholder.Description;
                IsMandatory = dataPlaceholder.IsMandatory;
            }
        }

        private string dataType;
        public string DataType
        {
            get => dataType;
            set
            {
                dataType = value;
                OnPropertyChanged();
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string description;
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged();
            }
        }

        private bool isMandatory;
        public bool IsMandatory
        {
            get => isMandatory;
            set
            {
                isMandatory = value;
                OnPropertyChanged();
            }
        }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(Name)
                   && string.IsNullOrWhiteSpace(Description)
                   && string.IsNullOrWhiteSpace(DataType);
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;
            if (string.IsNullOrWhiteSpace(DataType))
                return false;
            return true;
        }

        public DataPlaceholder Build()
        {
            return new DataPlaceholder(
                DataType.Trim(),
                Name.Trim(),
                Description,
                IsMandatory);
        }
    }
}