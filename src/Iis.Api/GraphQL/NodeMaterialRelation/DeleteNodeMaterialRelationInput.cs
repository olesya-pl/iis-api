using System;
using HotChocolate;
using HotChocolate.Types;
using Iis.DataModel.Materials;

namespace IIS.Core.GraphQL.NodeMaterialRelation
{
    public class DeleteNodeMaterialRelationInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid NodeId { get; set; }
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid MaterialId { get; set; }
        public MaterialNodeLinkType NodeLinkType { get; set; }
    }
}