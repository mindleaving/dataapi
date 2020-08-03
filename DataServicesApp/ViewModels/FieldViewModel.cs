using System;
using DataServicesApp.Models;
using SharedViewModels.ViewModels;

namespace DataServicesApp.ViewModels
{
    public class FieldViewModel : NotifyPropertyChangedBase
    {
        public string Path
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string As
        {
            get => GetValue<string>();
            set
            {
                if (value == "DataApiId")
                    throw new ArgumentOutOfRangeException(nameof(value), "As-value must not be 'DataApiId'");
                SetValue(value);
            }
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Path))
                return false;
            return true;
        }

        public DataServiceDefinition.Field Build()
        {
            return new DataServiceDefinition.Field(Path, As);
        }
    }
}
