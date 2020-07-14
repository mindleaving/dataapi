using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Commons.Extensions;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures;
using DataServicesApp.Helpers;
using DataServicesApp.Models;
using DataServicesApp.Workflow;
using SharedViewModels.Helpers;
using SharedViewModels.ViewModels;

namespace DataServicesApp.ViewModels
{
    public class SqlExportViewModel : NotifyPropertyChangedBase
    {
        private readonly IObjectDatabase<DataServiceDefinition> dataServiceDefinitionDatabase;
        private readonly IDataTypeList dataTypeList;
        private readonly IUsernameProxy usernameProxy;
        private readonly ISqlExpressionValidator sqlExpressionValidator;

        public SqlExportViewModel(
            IObjectDatabase<DataServiceDefinition> dataServiceDefinitionDatabase,
            IDataTypeList dataTypeList,
            IUsernameProxy usernameProxy,
            ISqlExpressionValidator sqlExpressionValidator)
        {
            this.dataServiceDefinitionDatabase = dataServiceDefinitionDatabase;
            this.dataTypeList = dataTypeList;
            this.usernameProxy = usernameProxy;
            this.sqlExpressionValidator = sqlExpressionValidator;
            Fields = new ObservableCollection<FieldViewModel>();
            if(!Fields.Any())
                Fields.Add(new FieldViewModel());
            CreateDataServiceCommand = new AsyncRelayCommand(CreateDataService, CanCreateDataService);
            TestFilterCommand = new AsyncRelayCommand(TestFilter, CanTestFilter);
            AddFieldCommand = new RelayCommand(AddField);
            DeleteFieldCommand = new RelayCommand(DeleteField, () => SelectedField != null);
        }

        public string TableName
        {
            get => GetValue<string>();
            set
            {
                if(!Regex.IsMatch(value, "^[a-zA-Z0-9_-]+$"))
                    throw new FormatException("Invalid table name");
                SetValue(value);
            }
        }

        private List<string> dataTypes;
        public List<string> DataTypes => dataTypes ?? (dataTypes = LoadDataTypes());

        public string SelectedDataType { get; set; }

        public string Filter
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public ObservableCollection<FieldViewModel> Fields { get; }
        public FieldViewModel SelectedField
        {
            get => GetValue<FieldViewModel>();
            set => SetValue(value);
        }

        public IAsyncCommand CreateDataServiceCommand { get; }
        public IAsyncCommand TestFilterCommand { get; }
        public ICommand AddFieldCommand { get; }
        public ICommand DeleteFieldCommand { get; }

        private bool CanCreateDataService()
        {
            if (string.IsNullOrWhiteSpace(TableName))
                return false;
            if (SelectedDataType == null)
                return false;
            if (!Fields.Any())
                return false;
            return true;
        }

        private async Task CreateDataService()
        {
            var (isFilterValid, errorText) = Task.Run(async () => await sqlExpressionValidator.ValidateWhereAsync(SelectedDataType, Filter)).Result;
            if (!isFilterValid)
            {
                StaticMessageBoxSpawner.Show($"Filter is NOT valid!\n{errorText}");
                return;
            }

            var username = usernameProxy.Username;

            var existingDataService = await dataServiceDefinitionDatabase.SearchAsync(
                $"Data.{nameof(DataServiceDefinition.OwnerInitials)} = '{username}' AND Data.{nameof(DataServiceDefinition.DataType)} = '{SelectedDataType}'", 1);
            if (existingDataService.Any())
            {
                StaticMessageBoxSpawner.Show("You already have a data service for that data type");
                return;
            }

            try
            {
                var id = IdGenerator.FromGuid();
                
                var target = new SqlDataServiceTarget(IdGenerator.FromGuid(), "dkhoeinnsql01", "PowerBiData", TableName, username);
                var dataService = new DataServiceDefinition(
                    id,
                    username,
                    SelectedDataType,
                    Fields.Where(x => x.IsValid()).Select(x => x.Build()).ToList(),
                    target,
                    !string.IsNullOrWhiteSpace(Filter) ? Filter : null);
                await dataServiceDefinitionDatabase.StoreAsync(dataService);
                StaticMessageBoxSpawner.Show("Data service created");
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show("Could not create data service: " + e.InnermostException().Message);
            }
        }

        private bool CanTestFilter()
        {
            return SelectedDataType != null;
        }

        private async Task TestFilter()
        {
            var (isValid,errorText) = await sqlExpressionValidator.ValidateWhereAsync(SelectedDataType, Filter);
            if (isValid)
                StaticMessageBoxSpawner.Show("Filter is valid");
            else
                StaticMessageBoxSpawner.Show($"Filter is NOT valid!\n{errorText}");
        }

        private void AddField()
        {
            Fields.Add(new FieldViewModel());
        }

        private void DeleteField()
        {
            Fields.Remove(SelectedField);
        }

        private List<string> LoadDataTypes()
        {
            return dataTypeList.GetCollections().Select(x => x.CollectionName).OrderBy(x => x).ToList();
        }
    }
}
