using Iis.Services.Contracts.Enums;

namespace IIS.Core.GraphQL.Users
{
    public class UserFilterInput
    {
        public string Suggestion { get; set; }
        public int UserStatus { get; set; }
    }
}