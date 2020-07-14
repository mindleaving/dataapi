using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Commons.Collections;
using Commons.Extensions;
using Commons.IO;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataManagement;
using FileHandlers.AdditionalInformation;
using FileHandlers.Models;
using FileHandlers.Objects;
using SharedViewModels.ViewModels;

namespace FileHandlers
{
    public class CsvFileHandler : IFileHandler
    {
        private readonly IObjectDatabase<CsvFile> csvFileDatabase;
        private readonly IObjectDatabase<ShortId> shortIdDatabase;
        private readonly IDataApiClient dataApiClient;

        public CsvFileHandler(
            IObjectDatabase<CsvFile> csvFileDatabase, 
            IObjectDatabase<ShortId> shortIdDatabase, 
            IDataApiClient dataApiClient)
        {
            this.csvFileDatabase = csvFileDatabase;
            this.shortIdDatabase = shortIdDatabase;
            
            this.dataApiClient = dataApiClient;
        }

        public string[] SupportedExtensions { get; } =
        {
            ".csv"
        };

        public bool RequiresAdditionalInformation { get; } = false;

        public IAdditionalInformationViewModel BuildAdditionalInformationViewModel() 
            => throw new InvalidOperationException("Additional information view model is only available if additional information is required");

        public async Task<FileHandlerResult> Handle(byte[] fileData, string fileName, string dataProjectId, object additionalInformation = null)
        {
            var csvFileId = IdGenerator.Sha1HashFromByteArray(fileData);
            var derivedDataReferences = new List<DataReference>
            {
                new DataReference(nameof(CsvFile), csvFileId)
            };
            var table = ReadCsv(fileData);
            if (!await csvFileDatabase.ExistsAsync(csvFileId))
            {
                var csvRows = new List<CsvRow>();
                foreach (var tableRow in table.Rows)
                {
                    var csvRow = new CsvRow();
                    foreach (var tableColumn in table.Columns)
                    {
                        var columnName = tableColumn.Name;
                        csvRow.Add(columnName, tableRow[columnName]);
                    }

                    csvRows.Add(csvRow);
                }

                var csvFile = new CsvFile(csvFileId, csvRows, fileName);
                await csvFileDatabase.StoreAsync(csvFile);
            }
            return new FileHandlerResult(derivedDataReferences, new List<IDerivedFileDataViewModel>
            {
                new CsvFileViewModel(
                    derivedDataReferences.Single(),
                    table, 
                    shortIdDatabase,
                    dataApiClient)
            });
        }

        private Table<string> ReadCsv(byte[] fileData)
        {
            var delimiter = DetectDelimiter(fileData);
            return CsvReader.ReadTable(() => new StreamReader(new MemoryStream(fileData)), x => x, delimiter: delimiter);
        }

        private static char DetectDelimiter(byte[] fileData)
        {
            using (var memoryStream = new MemoryStream(fileData))
            using (var streamReader = new StreamReader(memoryStream))
            {
                var firstLine = streamReader.ReadLine();
                if (string.IsNullOrEmpty(firstLine))
                    return ';';
                var secondLine = streamReader.ReadLine();
                var candidateDelimiters = new[] {',', ';', '|', '#'};
                var delimiterStatistics = new Dictionary<char, int>();
                foreach (var delimiter in candidateDelimiters)
                {
                    var firstLineSplits = firstLine.Split(delimiter);
                    if (!string.IsNullOrEmpty(secondLine))
                    {
                        var secondLineSplits = secondLine.Split(delimiter);
                        if(secondLineSplits.Length != firstLineSplits.Length)
                            continue;
                    }
                    delimiterStatistics.Add(delimiter, firstLineSplits.Length);
                }
                return delimiterStatistics.Count != 0 
                    ? delimiterStatistics.MaximumItem(kvp => kvp.Value).Key 
                    : ';';
            }
        }
    }
}
