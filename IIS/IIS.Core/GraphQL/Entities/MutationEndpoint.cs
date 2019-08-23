using HotChocolate;

namespace IIS.Core.GraphQL.Entities
{
    public class MutationEndpoint
    {
        [GraphQLType(typeof(MutationType))] public string Entities => "Entities";
    }
}
