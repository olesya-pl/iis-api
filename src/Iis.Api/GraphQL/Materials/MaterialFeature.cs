using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialFeatureNode
    {
        public string Id { get; set; }
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid NodeTypeId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
