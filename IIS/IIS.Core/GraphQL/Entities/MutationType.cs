using System.Linq;
using HotChocolate.Types;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;

namespace IIS.Core.GraphQL.Entities
{
    public class MutationType : ObjectType
    {
        private readonly IOntologyTypesService _ontologyTypesService;
        private readonly IOntologyFieldPopulator _populator;
        private readonly TypeRepository _typeRepository;

        public MutationType(IOntologyTypesService ontologyTypesService, TypeRepository typeRepository,
            IOntologyFieldPopulator populator)
        {
            _ontologyTypesService = ontologyTypesService;
            _typeRepository = typeRepository;
            _populator = populator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(nameof(MutationEndpoint));
            var typesToPopulate = _ontologyTypesService.EntityTypes
                .Where(t => t.CreateMeta().ExposeOnApi != false);
            typesToPopulate = typesToPopulate.Where(t => !t.IsAbstract);
            _populator.PopulateFields(descriptor, typesToPopulate,
                Operation.Create, Operation.Update, Operation.Delete);
        }
    }
}
