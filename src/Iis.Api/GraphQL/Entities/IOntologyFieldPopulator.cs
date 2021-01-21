using System;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities.InputTypes;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Ontology;
using Iis.Domain;

namespace IIS.Core.GraphQL.Entities
{
    public interface IOntologyFieldPopulator
    {
        void AddFields(IObjectTypeDescriptor descriptor, IEntityTypeModel type, Operation operation);
    }

    public class OntologyFieldPopulator : IOntologyFieldPopulator
    {
        private readonly TypeRepository _repository;

        public OntologyFieldPopulator(TypeRepository repository)
        {
            _repository = repository;
        }

        public void AddFields(IObjectTypeDescriptor descriptor, IEntityTypeModel type, Operation operation)
        {
            switch (operation)
            {
                case Operation.Read:
                    AddQueryFields(descriptor, type);
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

        private string GetName(Operation operation, IEntityTypeModel type)
        {
            return $"{operation.ToLower()}Entity{type.Name}";
        }

        public void AddCreateFields(IObjectTypeDescriptor descriptor, IEntityTypeModel type)
        {
            var operation = Operation.Create;
            var name = type.Name;
            descriptor.Field(GetName(operation, type))
                .Type(_repository.GetMutatorResponseType(operation, type))
//                .Argument("data", d => d.Type(new NonNullType(CreateObjectType(type)))) // fail
//                .Argument("data", d => d.Type<NonNullType<StringType>>()) // good
//                .Argument("data", d => d.Type(TypeProvider.Scalars[Core.Ontology.ScalarType.String])) // ok
//                .Argument("data", d => d.Type(new NonNullType(TypeProvider.Scalars[Core.Ontology.ScalarType.String]))) // fail
                .Argument("data", d =>
                    d.Type(_repository.GetMutatorInputType(operation, type)))
                .Resolver(ctx => ctx.Service<IOntologyMutationResolver>().CreateEntity(ctx, name));
        }

        public void AddUpdateFields(IObjectTypeDescriptor descriptor, IEntityTypeModel type)
        {
            var operation = Operation.Update;
            var name = type.Name;
            descriptor.Field(GetName(operation, type))
                .Type(_repository.GetMutatorResponseType(operation, type))
                .Argument("id", d => d.Type<NonNullType<IdType>>())
                .Argument("data", d =>
                    d.Type(_repository.GetMutatorInputType(operation, type)))
                .Resolver(ctx => ctx.Service<IOntologyMutationResolver>().UpdateEntity(ctx, name));
        }

        public void AddDeleteFields(IObjectTypeDescriptor descriptor, IEntityTypeModel type)
        {
            var operation = Operation.Delete;
            var name = type.Name;
            descriptor.Field(GetName(operation, type))
                .Type(_repository.GetMutatorResponseType(operation, type))
                .Argument("id", d => d.Type<NonNullType<IdType>>())
                .Resolver(ctx => ctx.Service<IOntologyMutationResolver>().DeleteEntity(ctx, name));
        }

        public void AddQueryFields(IObjectTypeDescriptor descriptor, IEntityTypeModel type)
        {
            var objectType = _repository.GetOntologyType(type);
            var collectionType =
                new CollectionType(OntologyObjectType.GetName(type), objectType); // TODO: new NonNullType() won't work
            
            descriptor.Field($"entity{type.Name}")
                .Type(objectType)
                .Argument("id", d => d.Type<NonNullType<IdType>>())
                .Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveEntity(ctx, type));

            var criteriaInput = new CriteriaInputType(type);
            descriptor.Field($"entity{type.Name}List")
                .Type(collectionType)
                .Argument("pagination", d => d.Type<NonNullType<InputObjectType<PaginationInput>>>())
                .Argument("filter", d => d.Type<InputObjectType<FilterInput>>())
                .Argument("criteria", d => d.Type(criteriaInput)) // TODO: it does nothing, backward compatibility with odyssey front
                .Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveEntityList(ctx, type));
        }
    }
}
