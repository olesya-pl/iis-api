using System.Collections.Generic;
using HotChocolate.Types;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class UpdateMutatorTypeCreator : MutatorTypeCreator
    {
        public UpdateMutatorTypeCreator(Dictionary<string, ObjectType> knownTypes) : base(knownTypes, "Update")
        {
        }

        public override void AddFields(IObjectTypeDescriptor descriptor, Type type)
        {
            descriptor.Field(Operation + type.Name)
                .Type(CreateResponse(type.Name))
                .Argument("id", d => d.Type<IdType>())
                .Argument("data", d => d.Type(CreateObjectType(type)))
                .ResolverNotImplemented();
        }
    }
}