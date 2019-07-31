using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.ObjectTypeCreators;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;

namespace IIS.Core.GraphQL.Mutations
{
    public class EntityMutationType : ObjectType
    {
        private readonly IOntologyProvider _ontologyProvider;
        private readonly GraphQlTypeCreator _graphQlTypeCreator;

        public EntityMutationType(IOntologyProvider ontologyProvider, GraphQlTypeCreator graphQlTypeCreator)
        {
            _ontologyProvider = ontologyProvider;
            _graphQlTypeCreator = graphQlTypeCreator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            var populator = new TypeFieldPopulator(_graphQlTypeCreator);
            descriptor.Name(nameof(EntityMutation));
            var typesToPopulate = _ontologyProvider.GetTypes().OfType<EntityType>()
                .Where(t => t.CreateMeta().ExposeOnApi != false);
            typesToPopulate = typesToPopulate.Where(t => !t.IsAbstract);
            populator.PopulateFields(descriptor, typesToPopulate,
                Operation.Create, Operation.Update, Operation.Delete);
        }
    }
}