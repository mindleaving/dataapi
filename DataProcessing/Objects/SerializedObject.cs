namespace DataProcessing.Objects
{
    public class SerializedObject
    {
        public SerializedObject(
            string id, 
            string dataType, 
            string json)
        {
            Id = id;
            DataType = dataType;
            Json = json;
        }

        public string Id { get; }
        public string DataType { get; }
        public string Json { get; }
    }
}
