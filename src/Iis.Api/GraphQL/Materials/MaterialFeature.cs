using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialFeature
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Relation { get; set; }
        public string Value { get; set; }
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid NodeId { get; set; }
    }
}
