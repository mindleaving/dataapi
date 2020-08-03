using System;
using MongoDB.Bson.Serialization.Attributes;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace DataAPI.Service.Objects
{
    public class View
    {
        public View(string id, string query, string submitter, DateTime? expires)
        {
            Id = id;
            Query = query;
            Submitter = submitter;
            Expires = expires;
        }

        [BsonId]
        public string Id { get; private set; }
        public string Query { get; private set; }
        public string Submitter { get; private set; }
        public DateTime? Expires { get; private set; }
    }
}
