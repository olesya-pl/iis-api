﻿using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.NodeMaterialRelation
{
    public class DeleteNodeMaterialRelationInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid NodeId { get; set; }
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid MaterialId { get; set; }
    }
}