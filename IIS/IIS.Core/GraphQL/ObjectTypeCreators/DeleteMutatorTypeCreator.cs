using System.Collections.Generic;
using HotChocolate.Types;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class DeleteMutatorTypeCreator : MutatorTypeCreator
    {
        public DeleteMutatorTypeCreator(Dictionary<string, ObjectType> knownTypes) : base(knownTypes, "Delete")
        {
        }

        public override void AddFields(IObjectTypeDescriptor descriptor, Type type)
        {
            descriptor.Field(Operation + type.Name)
                .Type(CreateResponse(type.Name))
                .Argument("id", d => d.Type<IdType>())
                .ResolverNotImplemented();
        }
    }
}