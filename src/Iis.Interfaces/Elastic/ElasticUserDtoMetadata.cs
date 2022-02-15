using System;
using Newtonsoft.Json;

namespace Iis.Services.Contracts.Dtos
{
    public class ElasticUserDtoMetadata
    {
        public ElasticUserDtoMetadata(Guid id, string username, int accessLevel)
        {
            Id = id;
            Username = username;
            AccessLevel = accessLevel;
        }

        public Guid Id { get; set; }
        public string Username { get; set; }
        public int AccessLevel { get; set; }
        [JsonProperty("_reserved")]
        public bool? Reserved { get; set; }
        public string SecurityLevels { get; set; }
    }
}
