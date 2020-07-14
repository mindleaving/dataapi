using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures.Attributes;
using DataAPI.DataStructures.Constants;

namespace DataAPI.DataStructures.DomainModels
{
    [DataApiCollection("DataProject")]
    public interface IDataProject<out TDataCollectionProtocol, TDataCollectionProtocolParamter,TDataPlaceholder> : IId
        where TDataCollectionProtocol: IDataCollectionProtocol<TDataCollectionProtocolParamter,TDataPlaceholder>
        where TDataCollectionProtocolParamter: IDataCollectionProtocolParameter
        where TDataPlaceholder: IDataPlaceholder
    {
        [Required]
        IdSourceSystem IdSourceSystem { get; }

        [Required]
        TDataCollectionProtocol Protocol { get; }

        Dictionary<string, string> ParameterResponse { get; }
    }
}