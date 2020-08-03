namespace DataAPI.Client
{
    public class ApiConfiguration
    {
        public ApiConfiguration(string serverAddress, ushort serverPort)
        {
            ServerAddress = serverAddress;
            ServerPort = serverPort;
        }

        public string ServerAddress { get; }
        public ushort ServerPort { get; }
    }
}