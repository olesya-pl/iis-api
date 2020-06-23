using System;
using HotChocolate;
using HotChocolate.Types;
using Iis.Domain;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialRelation
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid NodeId { get; set; }
        public EntityMaterialRelation NodeRelation { get; set; }
    }
}
