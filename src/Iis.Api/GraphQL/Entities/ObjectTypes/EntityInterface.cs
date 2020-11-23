using HotChocolate.Types;
using IIS.Core.GraphQL.Entities.Resolvers;

namespace IIS.Core.GraphQL.Entities.ObjectTypes
{
    // Explicit interface declaration, that would be implemented by each INodeTypeModel
    public class EntityInterface : InterfaceType
    {
        protected override void Configure(IInterfaceTypeDescriptor descriptor)
        {
            descriptor.Field("id").Type<NonNullType<IdType>>();
            descriptor.Field("createdAt").Type<NonNullType<DateTimeType>>();
            descriptor.Field("updatedAt").Type<NonNullType<DateTimeType>>();
            descriptor.Field("_relation").Type<RelationType>();
            descriptor.Description("Interface that is implemented by each ontology type");
            descriptor.ResolveAbstractType((ctx, obj) =>
                ctx.Service<IOntologyQueryResolver>().ResolveAbstractType(ctx, obj));
        }
    }
}
