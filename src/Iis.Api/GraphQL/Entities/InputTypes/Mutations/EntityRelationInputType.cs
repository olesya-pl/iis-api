using HotChocolate.Types;

namespace IIS.Core.GraphQL.Entities.InputTypes.Mutations
{
    public class EntityRelationInputType : InputObjectType
    {
        protected override void Configure(IInputObjectTypeDescriptor descriptor)
        {
            descriptor.Field("targetId").Type<NonNullType<IdType>>();
        }
    }
}
