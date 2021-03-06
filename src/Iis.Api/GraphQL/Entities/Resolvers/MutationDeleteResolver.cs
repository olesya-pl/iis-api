using System;
using System.Threading.Tasks;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using IIS.Core.Ontology;
using Iis.Domain;
using IIS.Domain;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public class MutationDeleteResolver
    {
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyModel _ontology;

        public MutationDeleteResolver(IOntologyService ontologyService, IOntologyModel ontology)
        {
            _ontologyService = ontologyService;
            _ontology = ontology;
        }

        public MutationDeleteResolver(IResolverContext ctx)
        {
            _ontology = ctx.Service<IOntologyModel>();
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
            var type = _ontology.GetEntityType(typeName);
            if (!node.Type.IsSubtypeOf(type))
                throw new QueryException($"Entity with id {id} is of type {node.Type.Name}, not of type {type.Name}");
            await _ontologyService.RemoveNodeAsync(node);
            return node;
        }
    }
}
