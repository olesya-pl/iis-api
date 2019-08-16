using System;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.Mutations.Resolvers
{
    public class DeleteMutationResolver
    {
        private IOntologyService _ontologyService;

        public DeleteMutationResolver(IOntologyService ontologyService)
        {
            _ontologyService = ontologyService;
        }

        public DeleteMutationResolver(IResolverContext ctx)
        {
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
            if (node?.Type.Name == typeName)
                await _ontologyService.RemoveNodeAsync(id);
            return node;
        }
    }
}
