using System;

namespace Iis.Services.Contracts.Dtos
{
    public class ElasticUserDto
    {
        public string Password { get; set; }
        public string[] Roles { get; set; }
        public bool Enabled { get; set; }
        public ElasticUserDtoMetadata Metadata { get; set; }
    }

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
    }
}
