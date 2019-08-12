using System;
using System.Collections.Generic;
using HotChocolate.Resolvers;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.Mutations
{
    public static class Resolvers
    {
        public static object CreateEntity(IResolverContext ctx, EntityType type)
        {
            var data = ctx.Argument<Dictionary<string, object>>("data");
            throw new NotImplementedException();
        }
        
        public static object UpdateEntity(IResolverContext ctx, EntityType type)
        {
            var id = ctx.Argument<Guid>("id");
            var data = ctx.Argument<Dictionary<string, object>>("data");
            throw new NotImplementedException();
        }
        
        public static object DeleteEntity(IResolverContext ctx, EntityType type)
        {
            var id = ctx.Argument<Guid>("id");
            throw new NotImplementedException();
        }
    }
}