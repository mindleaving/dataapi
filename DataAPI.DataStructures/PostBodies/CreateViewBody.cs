using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.PostBodies
{
    public class CreateViewBody
    {
        [JsonConstructor]
        public CreateViewBody(
            string query,
            DateTime? expires,
            string viewId = null)
        {
            Query = query;
            Expires = expires;
            ViewId = viewId;
        }

        [Required]
        public string Query { get; }

        public DateTime? Expires { get; }

        public string ViewId { get; }
    }
}
