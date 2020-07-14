using System;
using System.Collections.Generic;
using DataAPI.DataStructures.DomainModels;
using Newtonsoft.Json;

namespace FileHandlers.Models
{
    public class ExcelWorkbook : IId
    {
        [JsonConstructor]
        public ExcelWorkbook(
            string id,
            List<ExcelWorksheet> worksheets,
            string filename)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Filename = filename;
            Worksheets = worksheets ?? new List<ExcelWorksheet>();
        }

        public string Id { get; private set; }
        public string Filename { get; private set; }
        public List<ExcelWorksheet> Worksheets { get; private set; }
    }
}
