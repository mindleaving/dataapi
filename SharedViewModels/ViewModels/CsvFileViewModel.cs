using System.Data;
using System.Linq;
using Commons.Collections;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;

namespace SharedViewModels.ViewModels
{
    public class CsvFileViewModel : IDerivedFileDataViewModel
    {
        public CsvFileViewModel(
            DataReference csvDataReference,
            Table<string> data,
            IObjectDatabase<ShortId> shortIdDatabase,
            IDataApiClient dataApiClient) 
        {
            Data = CreateDataGridColumns(data);
            ShortIdEditViewModel = new ShortIdEditViewModel(csvDataReference, shortIdDatabase, dataApiClient);
        }

        private DataTable CreateDataGridColumns(Table<string> table)
        {
            var dataTable = new DataTable();
            foreach (var column in table.Columns)
            {
                dataTable.Columns.Add(column.Name, typeof(string));
            }

            foreach (var row in table.Rows.Take(20))
            {
                dataTable.Rows.Add(row.Cast<object>().ToArray());
            }
            return dataTable;
        }

        public DataTable Data { get; }
        public ShortIdEditViewModel ShortIdEditViewModel { get; }
    }
}
