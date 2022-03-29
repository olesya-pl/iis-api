using System;

namespace IIS.Core.GraphQL.Users
{
    public class UserFilterInput
    {
        public string Suggestion { get; set; }
        public int UserStatus { get; set; }
        public Guid? RoleId { get; set; }
    }
}