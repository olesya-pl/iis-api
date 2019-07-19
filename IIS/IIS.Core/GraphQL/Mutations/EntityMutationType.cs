using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.ObjectTypeCreators;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.Mutations
{
    public class EntityMutationType : ObjectType
    {
        private readonly IOntologyProvider _ontologyProvider;
        private readonly IGraphQlOntologyTypeProvider _graphQlOntologyTypeProvider;

        public EntityMutationType(IOntologyProvider ontologyProvider, IGraphQlOntologyTypeProvider graphQlOntologyTypeProvider)
        {
            _ontologyProvider = ontologyProvider;
            _graphQlOntologyTypeProvider = graphQlOntologyTypeProvider;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            var creators = new MutatorTypeCreator[]
            {
                new CreateMutatorTypeCreator(_graphQlOntologyTypeProvider.OntologyTypes),
                new UpdateMutatorTypeCreator(_graphQlOntologyTypeProvider.OntologyTypes),
                new DeleteMutatorTypeCreator(_graphQlOntologyTypeProvider.OntologyTypes),
            };
            var types = _ontologyProvider.GetTypes().OfType<EntityType>();
            descriptor.Name(nameof(EntityMutation));
            descriptor.Field("_typesCount").Type<IntType>().Resolver(types.Count());
            foreach (var type in types)
                foreach (var creator in creators)
                    creator.AddFields(descriptor, type);
        }
    }
}