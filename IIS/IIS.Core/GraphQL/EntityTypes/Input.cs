using HotChocolate;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class EntityTypesFilter
    {
        [GraphQLNonNullType] public string Parent { get; set; }
    }
}
