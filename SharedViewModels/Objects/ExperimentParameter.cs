using System;

namespace SharedViewModels.Objects
{
    public class ExperimentParameter : IExperimentParameter
    {
        public ExperimentParameter(
            string id, 
            string displayName,
            Type type, 
            string value, 
            string unit = null)
        {
            ID = id;
            DisplayName = displayName;
            Type = type;
            Value = value;
            Unit = unit;
        }

        public string ID { get; }
        public string DisplayName { get; }
        public Type Type { get; }
        public string Value { get; set; }
        public string Unit { get; }
    }
}