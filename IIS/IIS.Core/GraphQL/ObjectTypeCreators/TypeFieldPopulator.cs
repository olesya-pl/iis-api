using System;
using HotChocolate.Types;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class TypeFieldPopulator : ITypeFieldPopulator
    {
        private GraphQlTypeCreator _creator;

        public TypeFieldPopulator(GraphQlTypeCreator creator)
        {
            _creator = creator;
        }

        private string GetName(Operation operation, EntityType type) => $"{operation.ToLower()}Entity{type.Name}";

        public void AddCreateFields(IObjectTypeDescriptor descriptor, EntityType type)
        {
            var operation = Operation.Create;
            descriptor.Field(GetName(operation, type))
                .Type(_creator.GetMutatorResponseType(operation, type))
//                .Argument("data", d => d.Type(new NonNullType(CreateObjectType(type)))) // fail
//                .Argument("data", d => d.Type<NonNullType<StringType>>()) // good
//                .Argument("data", d => d.Type(TypeProvider.Scalars[Core.Ontology.ScalarType.String])) // ok
//                .Argument("data", d => d.Type(new NonNullType(TypeProvider.Scalars[Core.Ontology.ScalarType.String]))) // fail
                .Argument("data", d =>
                    d.Type(_creator.GetMutatorInputType(operation, type)))
                .ResolverNotImplemented();
        }
        
        public void AddUpdateFields(IObjectTypeDescriptor descriptor, EntityType type)
        {
            var operation = Operation.Update;
            descriptor.Field(GetName(operation, type))
                .Type(_creator.GetMutatorResponseType(operation, type))
                .Argument("id", d => d.Type<NonNullType<IdType>>())
                .Argument("data", d =>
                    d.Type(_creator.GetMutatorInputType(operation, type)))
                .ResolverNotImplemented();
        }
        
        public void AddDeleteFields(IObjectTypeDescriptor descriptor, EntityType type)
        {
            var operation = Operation.Delete;
            descriptor.Field(GetName(operation, type))
                .Type(_creator.GetMutatorResponseType(operation, type))
                .Argument("id", d => d.Type<NonNullType<IdType>>())
                .ResolverNotImplemented();
        }

        public void AddFields(IObjectTypeDescriptor descriptor, EntityType type, Operation operation)
        {
            switch (operation)
            {
                case Operation.Read:
                    throw new NotImplementedException();
                    break;
                case Operation.Create:
                    AddCreateFields(descriptor, type);
                    break;
                case Operation.Update:
                    AddUpdateFields(descriptor, type);
                    break;
                case Operation.Delete:
                    AddDeleteFields(descriptor, type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }
        }
    }
}