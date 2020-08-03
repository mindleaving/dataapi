using System;
using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures;
using Newtonsoft.Json;

namespace DataAPI.Client.Test.Models
{
    public class Location : IId, IEquatable<Location>
    {
        [JsonConstructor]
        public Location(string site, string room)
        {
            Site = site;
            Room = room;
        }

        [Required]
        public string Id => $"{Site}_{Room}";
        [Required]
        public string Site { get; private set; }
        [Required]
        public string Room { get; private set; }

        public bool Equals(Location other)
        {
            return Id == other?.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Location) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}