using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures;
using Newtonsoft.Json;

namespace FileHandlers.Models
{
    public class CsvFile : IId
    {
        [JsonConstructor]
        public CsvFile(
            string id,
            List<CsvRow> rows,
            string filename)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Rows = rows ?? throw new ArgumentNullException(nameof(rows));
            Filename = filename;
        }

        [Required]
        public string Id { get; private set; }
        public string Filename { get; private set; }
        [Required]
        public List<CsvRow> Rows { get; private set; }
    }

    public class CsvRow : Dictionary<string, string>
    {
        // DO NOT ADD PROPERTIES
    }
}
