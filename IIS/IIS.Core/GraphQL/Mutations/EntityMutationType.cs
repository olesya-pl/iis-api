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
        private readonly GraphQlTypeCreator _graphQlTypeCreator;

        public EntityMutationType(IOntologyProvider ontologyProvider, GraphQlTypeCreator graphQlTypeCreator)
        {
            _ontologyProvider = ontologyProvider;
            _graphQlTypeCreator = graphQlTypeCreator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            var creator = new CompositeMutatorTypeCreator(_graphQlTypeCreator);
            descriptor.Name(nameof(EntityMutation));
            descriptor.PopulateFields(_ontologyProvider, creator);
        }
    }
}