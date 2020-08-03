using System.Collections.Generic;
using DataAPI.DataStructures.Attributes;

namespace DataAPI.DataStructures.DomainModels
{
    [DataApiCollection("DataCollectionProtocol")]
    public interface IDataCollectionProtocol<TDataCollectionProtocolParameter,TDataPlaceholder> : IId
        where TDataCollectionProtocolParameter: IDataCollectionProtocolParameter
        where TDataPlaceholder: IDataPlaceholder
    {
        List<TDataCollectionProtocolParameter> Parameters { get; }
        List<TDataPlaceholder> ExpectedData { get; }
    }
}