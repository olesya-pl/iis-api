using System;
using System.Threading.Tasks;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public class MutationDeleteResolver
    {
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyProvider _ontologyProvider;

        public MutationDeleteResolver(IOntologyService ontologyService, IOntologyProvider ontologyProvider)
        {
            _ontologyService = ontologyService;
            _ontologyProvider = ontologyProvider;
        }

        public MutationDeleteResolver(IResolverContext ctx)
        {
            _ontologyProvider = ctx.Service<IOntologyProvider>();
            _ontologyService = ctx.Service<IOntologyService>();
        }

        public async Task<Entity> DeleteEntity(IResolverContext ctx, string typeName)
        {
            var id = ctx.Argument<Guid>("id");
            return await DeleteEntity(id, typeName);
        }

        public async Task<Entity> DeleteEntity(Guid id, string typeName)
        {
            var node = (Entity) await _ontologyService.LoadNodesAsync(id, null); // load only type
            if (node == null)
                throw new QueryException($"Entity with id {id} was not found");
            var ontology = await _ontologyProvider.GetOntologyAsync();
            var type = ontology.GetEntityType(typeName);
            if (type.IsAssignableFrom(node.Type))
                throw new QueryException($"Entity with id {id} is of type {node.Type.Name}, not of type {type.Name}");
            await _ontologyService.RemoveNodeAsync(node);
            return node;
        }
    }
}
