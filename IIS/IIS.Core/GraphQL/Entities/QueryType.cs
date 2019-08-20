using System.Linq;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities.InputTypes;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;

namespace IIS.Core.GraphQL.Entities
{
    public class QueryType : ObjectType
    {
        private readonly IOntologyTypesService _ontologyTypesService;
        private readonly IOntologyFieldPopulator _populator;
        private readonly TypeRepository _repository;

        public QueryType(IOntologyTypesService ontologyTypesService, TypeRepository repository,
            IOntologyFieldPopulator populator)
        {
            _ontologyTypesService = ontologyTypesService;
            _repository = repository;
            _populator = populator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(nameof(QueryEndpoint));
            var typesToPopulate = _ontologyTypesService.EntityTypes
                .Where(t => t.CreateMeta().ExposeOnApi != false);
            _populator.PopulateFields(descriptor, typesToPopulate, Operation.Read);
            descriptor.Field("allEntities")
                .Type(new CollectionType("AllEntities", _repository.GetType<EntityInterface>()))
                .Argument("pagination", d => d.Type<NonNullType<InputObjectType<PaginationInput>>>())
                .Argument("filter", d => d.Type<InputObjectType<FilterInput>>())
                .ResolverNotImplemented();
        }
    }
}
