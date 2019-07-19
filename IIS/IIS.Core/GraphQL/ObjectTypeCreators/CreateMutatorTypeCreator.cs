using System.Collections.Generic;
using HotChocolate.Types;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class CreateMutatorTypeCreator : MutatorTypeCreator
    {
        public CreateMutatorTypeCreator(Dictionary<string, ObjectType> knownTypes) : base(knownTypes, "Create")
        {
        }

        public override void AddFields(IObjectTypeDescriptor descriptor, Type type)
        {
            descriptor.Field(Operation + type.Name)
                .Type(CreateResponse(type.Name))
                .Argument("data", d => d.Type(CreateObjectType(type)))
                .ResolverNotImplemented();
        }
    }
}