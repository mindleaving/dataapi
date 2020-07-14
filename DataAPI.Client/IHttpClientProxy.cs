using System;
using System.Net.Http;

namespace DataAPI.Client
{
    public interface IHttpClientProxy : IDisposable
    {
        HttpClient Client { get; }
        bool UseActiveDirectoryAuthorization { get; set; }
    }
}