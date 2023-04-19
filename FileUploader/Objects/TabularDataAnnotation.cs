using System.Collections.Generic;

namespace FileUploader.Objects
{
    public class TabularDataAnnotation
    {
        public int StartRow { get; set; }
        public int EndRow { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }

        public StopCriteria RowScanStopCriteria { get; set; }
        public StopCriteria ColumnScanStopCriteria { get; set; }

        public bool FirstRowIsHeader { get; set; }
        public bool FirstColumnIsLabel { get; set; }

        public Dictionary<string, SharedColumn> HeaderMapping { get; set; }
    }

    public class StopCriteria
    {
        public StopCriteriaType Type { get; set; }
        public int Count { get; set; }
    }

    public enum StopCriteriaType
    {
        Fixed,
        Blank
    }
}
