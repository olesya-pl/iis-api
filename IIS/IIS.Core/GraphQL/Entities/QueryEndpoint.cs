using HotChocolate;

namespace IIS.Core.GraphQL.Entities
{
    public class QueryEndpoint
    {
        [GraphQLType(typeof(QueryType))] public string Entities => "Entities";
    }
}
