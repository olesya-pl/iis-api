using System;
using HotChocolate;
using HotChocolate.Types;
using Iis.DataModel.Materials;

namespace IIS.Core.GraphQL.NodeMaterialRelation
{
    public class NodeMaterialRelationInput
    {
        [GraphQLType(typeof(ListType<NonNullType<IdType>>))]
        public Guid[] NodeIds { get; set; }
        [GraphQLType(typeof(ListType<NonNullType<IdType>>))]
        public Guid[] MaterialIds { get; set; }
        public MaterialNodeLinkType NodeLinkType { get; set; }
    }
}
