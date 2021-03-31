using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Materials
{
    public class AssignMaterialsOperatorInput
    {
        [GraphQLType(typeof(ListType<NonNullType<IdType>>))]
        public Guid[] MaterialIds { get; set; }
        public Guid AssigneeId { get; set; }
    }
}
