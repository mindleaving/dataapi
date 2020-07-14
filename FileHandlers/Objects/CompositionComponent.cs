using Commons.Physics;

namespace FileHandlers.Objects
{
    public class CompositionComponent
    {
        public string ComponentId { get; }
        public string MaterialBatch { get; }
        public UnitValue Amount { get; }

        public CompositionComponent(string componentId, string materialBatch, UnitValue amount)
        {
            ComponentId = componentId;
            MaterialBatch = materialBatch;
            Amount = amount;
        }
    }
}