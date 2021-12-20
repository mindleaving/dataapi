using DataAPI.DataStructures;

namespace DataProcessing.Test.Models
{
    public class Location : IId
    {
        public Location(string site, string room)
        {
            Site = site;
            Room = room;
        }

        public string Id => $"{Site}_{Room}";
        public string Site { get; set; }
        public string Room { get; set; }
    }
}
