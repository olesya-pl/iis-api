using IIS.Core.GraphQL.Roles;

namespace Iis.Api.GraphQL.Roles
{
    public class RoleResponse
    {
        public Role Role { get; set; }
        public bool AlreadyExists { get; set; }
    }
}