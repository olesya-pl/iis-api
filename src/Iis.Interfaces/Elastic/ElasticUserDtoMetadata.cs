using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Iis.Services.Contracts.Dtos
{
    public class ElasticUserMetadataDto
    {
        public ElasticUserMetadataDto(Guid id, string username, int accessLevel)
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
        public IReadOnlyList<ElasticUserChannelDto> Channels { get; set; }
    }
}
