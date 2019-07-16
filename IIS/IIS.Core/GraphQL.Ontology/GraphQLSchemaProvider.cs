using System;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Types;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.Ontology
{
    public class GraphQLSchemaProvider : IGraphQLSchemaProvider
    {
        private readonly IOntologyProvider _ontologyProvider;

        public GraphQLSchemaProvider(IOntologyProvider ontologyProvider)
        {
            _ontologyProvider = ontologyProvider;
        }

        public async Task<ISchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            var ontology = await _ontologyProvider.GetTypesAsync(cancellationToken);

            // todo: Build graphql mutation schema based on the ontology
            throw new NotImplementedException();
        }
    }
}
