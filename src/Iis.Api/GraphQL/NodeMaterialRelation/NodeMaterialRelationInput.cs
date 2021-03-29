﻿using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.NodeMaterialRelation
{
    public class NodeMaterialRelationInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid NodeId { get; set; }
        [GraphQLType(typeof(ListType<NonNullType<IdType>>))]
        public Guid[] MaterialIds { get; set; }
    }
}
