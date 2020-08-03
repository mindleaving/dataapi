namespace SharedViewModels.Objects
{
    public class ObjectWithDisplayName
    {
        public ObjectWithDisplayName(object o, string displayName)
        {
            Object = o;
            DisplayName = displayName;
        }

        public object Object { get; }
        public string DisplayName { get; }
    }
}
