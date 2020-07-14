namespace FileHandlers.Models
{
    public class ExcelCell
    {
        public ExcelCell(int row, int column, string value)
        {
            Row = row;
            Column = column;
            Value = value;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }
        public string Value { get; private set; }
    }
}