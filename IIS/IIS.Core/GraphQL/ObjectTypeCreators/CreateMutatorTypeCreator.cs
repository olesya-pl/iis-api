using System.Collections.Generic;
using HotChocolate.Types;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class CreateMutatorTypeCreator : MutatorTypeCreator
    {
        public CreateMutatorTypeCreator(IGraphQlTypeProvider typeProvider) : base(typeProvider, "Create")
        {
        }

        public override void AddFields(IObjectTypeDescriptor descriptor, EntityType type)
        {
            descriptor.Field(Operation + type.Name)
                .Type(CreateResponse(type.Name))
//                .Argument("data", d => d.Type(new NonNullType(CreateObjectType(type)))) // fail
//                .Argument("data", d => d.Type<NonNullType<StringType>>()) // good
//                .Argument("data", d => d.Type(TypeProvider.Scalars[Core.Ontology.ScalarType.String])) // ok
//                .Argument("data", d => d.Type(new NonNullType(TypeProvider.Scalars[Core.Ontology.ScalarType.String]))) // fail
                .Argument("data", d => d.Type(CreateObjectType(type)))
                .ResolverNotImplemented();
        }
    }
}