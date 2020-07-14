namespace DataProcessingService.Web.Configuration
{
    public class DataApiSettings
    {
        public string ServerAddress { get; set; }
        public ushort ServerPort { get; set; }
        public string Username { get; set; }
        public string PasswordEnvironmentVariableName { get; set; }
    }
}
