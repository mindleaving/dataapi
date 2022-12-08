namespace DataAPI.Client.Test.Models
{
    internal class Machine
    {
        public Machine(
            Location location)
        {
            Location = location;
        }

        public Location Location { get; }
    }
}
