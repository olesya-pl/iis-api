using HotChocolate;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class EntityTypesFilter
    {
        public string Parent { get; set; }
        public bool ConcreteTypes { get; set; }
    }
}
