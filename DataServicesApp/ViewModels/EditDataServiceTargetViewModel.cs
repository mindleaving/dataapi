using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.Distribution;
using SharedViewModels.ViewModels;

namespace DataServicesApp.ViewModels
{
    public class EditDataServiceTargetViewModel : NotifyPropertyChangedBase
    {
        private readonly IObjectDatabase<IDataServiceTarget> dataServiceTargetDatabase;

        public EditDataServiceTargetViewModel(IObjectDatabase<IDataServiceTarget> dataServiceTargetDatabase)
        {
            this.dataServiceTargetDatabase = dataServiceTargetDatabase;
            SaveCommand = new AsyncRelayCommand(Save, CanSave);
        }

        public List<DataServiceTargetType> DataServiceTargetTypes { get; } 
            = EnumExtensions.GetValues<DataServiceTargetType>().Except(new[] {DataServiceTargetType.Undefined}).ToList();

        public DataServiceTargetType SelectedDataServiceTargetType
        {
            get => GetValue<DataServiceTargetType>();
            set
            {
                SetValue(value);
                switch (SelectedDataServiceTargetType)
                {
                    case DataServiceTargetType.File:
                        DataServiceTargetViewModel = new FileDataServiceTargetViewModel();
                        break;
                    case DataServiceTargetType.Sql:
                        DataServiceTargetViewModel = new SqlDataServiceTargetViewModel();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public IDataServiceTargetViewModel DataServiceTargetViewModel
        {
            get => GetValue<IDataServiceTargetViewModel>();
            private set => SetValue(value);
        }

        public IAsyncCommand SaveCommand { get; }

        private bool CanSave()
        {
            if (DataServiceTargetViewModel == null)
                return false;
            return DataServiceTargetViewModel.IsValid(out _);
        }

        private async Task Save()
        {
            var dataServiceTarget = DataServiceTargetViewModel.Build();
            await dataServiceTargetDatabase.StoreAsync(dataServiceTarget);
        }
    }
}
