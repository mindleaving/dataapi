namespace FileUploader.Objects
{
    public class SharedColumn
    {
        public SharedColumn(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }
        // TODO: ExpectedDataType: number, text, other
    }
}