namespace FileUploader.Objects
{
    public class SharedValue
    {
        public SharedValue(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }
        // TODO: Expected type
    }
}
