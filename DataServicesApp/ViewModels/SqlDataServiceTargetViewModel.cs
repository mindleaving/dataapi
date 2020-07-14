using DataAPI.DataStructures;
using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.DomainModels;
using DataServicesApp.Models;
using SharedViewModels.ViewModels;

namespace DataServicesApp.ViewModels
{
    public class SqlDataServiceTargetViewModel : NotifyPropertyChangedBase, IDataServiceTargetViewModel
    {
        public SqlDataServiceTargetViewModel(SqlDataServiceTarget model = null)
        {
            if (model != null)
            {
                Id = model.Id;
                DataSource = model.DataSource;
                DatabaseName = model.DatabaseName;
                TableName = model.TableName;
                Username = model.Username;
            }
            else
            {
                Id = IdGenerator.FromGuid();
            }
        }

        public DataServiceTargetType Type { get; } = DataServiceTargetType.Sql;
        public string Description => $"{DataSource} {DatabaseName} {TableName} ({Username})";

        public string Id { get; }

        public string DataSource
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string DatabaseName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string TableName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string Username
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public bool IsValid(out string errorText)
        {
            if (string.IsNullOrWhiteSpace(DataSource))
            {
                errorText = "Invalid data source";
                return false;
            }
            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                errorText = "Invalid database name";
                return false;
            }
            if (string.IsNullOrWhiteSpace(TableName))
            {
                errorText = "Invalid table name";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Username))
            {
                errorText = "Invalid username";
                return false;
            }
            errorText = null;
            return true;
        }

        public IDataServiceTarget Build()
        {
            return new SqlDataServiceTarget(Id, DataSource, DatabaseName, TableName, Username);
        }
    }
}
