using System.Collections.Generic;

namespace Iis.Services.Contracts.Dtos
{
    public class ElasticUserDto
    {
        public string Password { get; set; }
        public string[] Roles { get; set; }
        public bool Enabled { get; set; }
        public IReadOnlyList<int> SecurityLevelIndexes { get; set; }
        public ElasticUserMetadataDto Metadata { get; set; }
    }
}
