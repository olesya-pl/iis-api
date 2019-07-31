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
            var parent = ctx.Parent<Node>();
            throw new NotImplementedException();
        }
        
        public static Task<object> ResolveAttributeRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var parent = ctx.Parent<Node>();
            throw new NotImplementedException();
        }
        
        public static async Task<Relation> ResolveParentRelation(IResolverContext ctx)
        {
            // resolver for "_relation" field on schema
            // todo: You should resolve parent-to-current entity relation here. However, ctx.Parent would return current entity.
            throw new NotImplementedException();
        }

        public static async Task<Guid> ResolveId(IResolverContext ctx)
        {
            return ctx.Parent<Node>().Id;
        }

        public static async Task<Node> ResolveEntity(IResolverContext ctx, EntityType type)
        {
            var id = ctx.Argument<Guid>("id");
            throw new NotImplementedException();
        }

        public static async Task<IEnumerable<Node>> ResolveEntityList(IResolverContext ctx, EntityType type)
        {
            throw new NotImplementedException();
        }
    }
}