using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.Distribution;
using DataServicesApp.Models;
using SharedViewModels.ViewModels;

namespace DataServicesApp.ViewModels
{
    public class DataServiceTargetWindowViewModel : NotifyPropertyChangedBase
    {
        private readonly IObjectDatabase<IDataServiceTarget> dataServiceTargetDatabase;

        public DataServiceTargetWindowViewModel(IObjectDatabase<IDataServiceTarget> dataServiceTargetDatabase)
        {
            this.dataServiceTargetDatabase = dataServiceTargetDatabase;
            EditViewModel = new EditDataServiceTargetViewModel(dataServiceTargetDatabase);
        }

        private List<IDataServiceTargetViewModel> targets;
        public List<IDataServiceTargetViewModel> Targets => targets ?? (targets = LoadDataServiceTargets());

        public EditDataServiceTargetViewModel EditViewModel { get; }

        private List<IDataServiceTargetViewModel> LoadDataServiceTargets()
        {
            return Task.Run(async () => await dataServiceTargetDatabase.GetManyAsync()).Result.Select(CreateViewModel).ToList();
        }

        private IDataServiceTargetViewModel CreateViewModel(IDataServiceTarget model)
        {
            switch (model.Type)
            {
                case DataServiceTargetType.File:
                    return new FileDataServiceTargetViewModel((FileDataServiceTarget) model);
                case DataServiceTargetType.Sql:
                    return new SqlDataServiceTargetViewModel((SqlDataServiceTarget) model);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
