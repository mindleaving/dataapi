using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace DataAPI.Client
{
    internal class DataApiHttpClientProxy : IHttpClientProxy
    {
        private bool IgnoreSslNameMismatch { get; }

        private HttpClientHandler httpClientHandler;
        public HttpClient Client { get; private set; }
        public bool UseActiveDirectoryAuthorization
        {
            get => httpClientHandler.UseDefaultCredentials;
            set
            {
                if(value == httpClientHandler.UseDefaultCredentials)
                    return;

                Client.Dispose();
                httpClientHandler.Dispose();

                InitializeNewHttpClientHandler(value);
                CreateNewClient();
            }
        }

        public DataApiHttpClientProxy(bool useActiveDirectory, bool ignoreSslNameMismatch = false)
        {
            if(ignoreSslNameMismatch)
                Console.WriteLine("WARNING: SSL name mismatch is ignored. This is a security hazard!!!");
            IgnoreSslNameMismatch = ignoreSslNameMismatch;
            InitializeNewHttpClientHandler(useActiveDirectory);
            CreateNewClient();
        }

        private void InitializeNewHttpClientHandler(bool useActiveDirectory)
        {
            httpClientHandler = new HttpClientHandler
            {
                UseDefaultCredentials = useActiveDirectory,
                ServerCertificateCustomValidationCallback = ValidateCertificate
            };
        }

        private void CreateNewClient()
        {
            Client = new HttpClient(httpClientHandler);
        }

        private bool ValidateCertificate(HttpRequestMessage requestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if(IgnoreSslNameMismatch && errors == SslPolicyErrors.RemoteCertificateNameMismatch)
                return true;
            return errors == SslPolicyErrors.None;
        }

        public void Dispose()
        {
            httpClientHandler.Dispose();
            Client.Dispose();
        }
    }
}
