using System;

namespace SharedViewModels.Objects
{
    public interface IExperimentParameter
    {
        string ID { get; }
        string DisplayName { get; }
        Type Type { get; }
        string Value { get; set; }
        string Unit { get; }
    }
}