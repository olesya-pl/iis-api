using HotChocolate.Types;

namespace IIS.Core.GraphQL.Entities.InputTypes.Mutations
{
    public class UpdateEntityRelationInputType : InputObjectType
    {
        protected override void Configure(IInputObjectTypeDescriptor descriptor)
        {
            descriptor.Field("targetId").Type<NonNullType<IdType>>();
            descriptor.Field("id").Type<NonNullType<IdType>>();
        }
    }
}
