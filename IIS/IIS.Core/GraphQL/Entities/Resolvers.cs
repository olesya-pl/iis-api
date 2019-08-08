using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.Entities
{
    public static class Resolvers
    {
        public static async Task<object> ResolveEntityRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var parent = ctx.Parent<Node>();
            var node = await ontologyService.LoadNodesAsync(parent, new[] { relationType });
            // return ??
            throw new NotImplementedException();
        }
        
        public static async Task<object> ResolveAttributeRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var parent = ctx.Parent<Node>();
            var node = await ontologyService.LoadNodesAsync(parent, new[] { relationType });
            // return ??
            throw new NotImplementedException();
        }

        public static async Task<Guid> ResolveId(IResolverContext ctx)
        {
            return ctx.Parent<Node>().Id;
        }

        public static async Task<Node> ResolveEntity(IResolverContext ctx, EntityType type)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var id = ctx.Argument<Guid>("id");
            var node = await ontologyService.LoadNodesAsync(new Entity(id), null);
            return node; // ???
            //throw new NotImplementedException();
        }

        public static async Task<IEnumerable<Node>> ResolveEntityList(IResolverContext ctx, EntityType type)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var list = await ontologyService.GetNodesByTypeAsync(type);
            return list;
            //throw new NotImplementedException();
        }
    }
}