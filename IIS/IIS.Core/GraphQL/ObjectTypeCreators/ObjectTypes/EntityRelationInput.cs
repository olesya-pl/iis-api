using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes
{
    public class EntityRelationInputType : InputObjectType
    {
        protected override void Configure(IInputObjectTypeDescriptor descriptor)
        {
            descriptor.Field("targetId").Type<NonNullType<IdType>>();
        }
    }

    public class UpdateEntityRelationInputType : InputObjectType
    {
        protected override void Configure(IInputObjectTypeDescriptor descriptor)
        {
            descriptor.Field("targetId").Type<NonNullType<IdType>>();
            descriptor.Field("id").Type<NonNullType<IdType>>();
        }
    }
}
