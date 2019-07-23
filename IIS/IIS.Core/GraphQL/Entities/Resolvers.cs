using System;
using HotChocolate.Resolvers;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.Entities
{
    public static class Resolvers
    {
        public static object ResolveRelation(IResolverContext context, EmbeddingRelationType relationType)
        {
            throw new NotImplementedException();
        }

        public static Guid ResolveId(IResolverContext context)
        {
            return context.Parent<Node>().Id;
        }
    }
}