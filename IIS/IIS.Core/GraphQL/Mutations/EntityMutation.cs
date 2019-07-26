using HotChocolate;

namespace IIS.Core.GraphQL.Mutations
{
    public class EntityMutation
    {
        [GraphQLType(typeof(EntityMutationType))]
        public string Entities => "";
    }
}