using System.Collections.Generic;

namespace FileUploader.Objects
{
    public class TabularDataAnnotation
    {
        public int StartRow { get; }
        public int EndRow { get; }
        public int StartColumn { get; }
        public int EndColumn { get; }

        public StopCriteria RowScanStopCriteria { get; }
        public StopCriteria ColumnScanStopCriteria { get; }

        public bool FirstRowIsHeader { get; }
        public bool FirstColumnIsLabel { get; }

        public Dictionary<string, SharedColumn> HeaderMapping { get; }
    }

    public class StopCriteria
    {
        public StopCriteriaType Type { get; }
        public int Count { get; }
    }

    public enum StopCriteriaType
    {
        Fixed,
        Blank
    }
}
