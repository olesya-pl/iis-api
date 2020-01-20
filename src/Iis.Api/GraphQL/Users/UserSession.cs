using HotChocolate;

namespace IIS.Core.GraphQL.Users
{
    public class UserSession
    {
        [GraphQLNonNullType] public User User { get; set; }
        [GraphQLNonNullType] public string Token { get; set; }
    }
}
