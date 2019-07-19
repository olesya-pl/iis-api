using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Mutations
{
    public class EntityRelationInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid TargetId { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
    }
}