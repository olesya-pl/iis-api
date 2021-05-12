using Iis.Services.Contracts.Enums;

namespace IIS.Core.GraphQL.Users
{
    public class UserFilterInput
    {
        public UserStatusType UserStatus { get; set; }
    }
}