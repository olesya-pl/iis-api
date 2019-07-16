using HotChocolate;

namespace IIS.Core.GraphQL.Entities
{
    public class Query
    {
        [GraphQLType(typeof(EntityQuery))]
        public object GetEntities()
        {
            return new { }; // Stub for generating dummy entity query schema
        }
    }
}