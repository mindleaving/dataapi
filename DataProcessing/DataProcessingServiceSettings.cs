namespace DataProcessing
{
    /// <summary>
    /// Settings read from appsettings.json
    /// </summary>
    public class DataProcessingServiceSettings
    {
        public string ProcessorDefinitionDirectory { get; set; }
        public string TaskDefinitionDirectory { get; set; }
    }
}
