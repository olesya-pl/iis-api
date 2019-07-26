using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes
{
    public class EntityRelationInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid TargetId { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
    }
    
    public class EntityRelationInputType : InputObjectType<EntityRelationInput>
    {
        
    }
}