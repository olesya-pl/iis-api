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
        private readonly IGraphQlTypeProvider _graphQlTypeProvider;

        public EntityMutationType(IOntologyProvider ontologyProvider, IGraphQlTypeProvider graphQlTypeProvider)
        {
            _ontologyProvider = ontologyProvider;
            _graphQlTypeProvider = graphQlTypeProvider;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            var creator = new CompositeMutatorTypeCreator(_graphQlTypeProvider);
            descriptor.Name(nameof(EntityMutation));
            descriptor.PopulateFields(_ontologyProvider, creator);
        }
    }
}