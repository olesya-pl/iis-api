using HotChocolate;

namespace IIS.Core.GraphQL.Entities
{
    public class EntityQuery
    {
        [GraphQLType(typeof(EntityQueryType))]
        public object GetEntities()
        {
            return new { }; // Stub for generating dummy entity query schema
        }
    }
}