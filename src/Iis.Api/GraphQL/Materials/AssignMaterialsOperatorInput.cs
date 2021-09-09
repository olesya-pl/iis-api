using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Materials
{
    public class AssignMaterialOperatorInput
    {
        [GraphQLType(typeof(ListType<NonNullType<IdType>>))]
        public Guid[] MaterialIds { get; set; }
        [GraphQLType(typeof(ListType<NonNullType<IdType>>))]
        public Guid[] AssigneeIds { get; set; }
    }
}