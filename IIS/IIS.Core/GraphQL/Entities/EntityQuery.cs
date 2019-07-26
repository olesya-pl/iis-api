using HotChocolate;

namespace IIS.Core.GraphQL.Entities
{
    public class EntityQuery
    {
        [GraphQLType(typeof(EntityQueryType))]
        public string GetEntities()
        {
            return ""; // Stub for generating dummy entity query schema
        }
    }
}