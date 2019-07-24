using System.Collections.Generic;
using HotChocolate.Types;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class UpdateMutatorTypeCreator : MutatorTypeCreator, ITypeFieldPopulator
    {
        public UpdateMutatorTypeCreator(IGraphQlTypeProvider typeProvider) : base(typeProvider, "Update")
        {
        }

        public override void AddFields(IObjectTypeDescriptor descriptor, EntityType type)
        {
            descriptor.Field(Operation + type.Name)
                .Type(CreateResponse(type.Name))
                .Argument("id", d => d.Type<NonNullType<IdType>>())
//                .Argument("data", d => d.Type(new NonNullType(CreateObjectType(type))))
                .Argument("data", d => d.Type(CreateObjectType(type)))
                .ResolverNotImplemented();
        }
    }
}