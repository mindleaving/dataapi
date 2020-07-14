using System;
using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.DomainModels;
using DataServicesApp.Models;
using SharedViewModels.ViewModels;

namespace DataServicesApp.ViewModels
{
    public class FileDataServiceTargetViewModel : NotifyPropertyChangedBase, IDataServiceTargetViewModel
    {
        public FileDataServiceTargetViewModel(FileDataServiceTarget model = null)
        {
            if (model != null)
            {
                Directory = model.Directory;
            }
        }

        public DataServiceTargetType Type { get; } = DataServiceTargetType.File;
        public string Description => $"{Directory}";

        public string Directory
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public bool IsValid(out string errorText)
        {
            if (string.IsNullOrWhiteSpace(Directory))
            {
                errorText = "No directory specified";
                return false;
            }

            if (!System.IO.Directory.Exists(Directory))
            {
                errorText = "Directory doesn't exist";
                return false;
            }

            errorText = null;
            return true;
        }

        public IDataServiceTarget Build()
        {
            throw new NotImplementedException();
        }
    }
}