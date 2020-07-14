using System.Collections.Generic;

namespace FileHandlers.Models
{
    public class ExcelWorksheet
    {
        public ExcelWorksheet(string name,
            List<ExcelCell> cells,
            int rowCount,
            int columnCount)
        {
            Name = name;
            RowCount = rowCount;
            ColumnCount = columnCount;
            Cells = cells ?? new List<ExcelCell>();
        }

        public string Name { get; }
        public List<ExcelCell> Cells { get; }
        public int RowCount { get; }
        public int ColumnCount { get; }
    }
}