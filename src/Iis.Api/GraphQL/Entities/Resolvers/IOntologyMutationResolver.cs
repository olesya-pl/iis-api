using System.Threading.Tasks;
using HotChocolate.Resolvers;
using IIS.Core.Ontology;
using Iis.Domain;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public interface IOntologyMutationResolver
    {
        Task<string> ResolveResponseType(IResolverContext ctx);
        Task<Entity> ResolveResponseDetails(IResolverContext ctx);
        Task<Entity> CreateEntity(IResolverContext ctx, string typeName);
        Task<Entity> UpdateEntity(IResolverContext ctx, string typeName);
        Task<Entity> DeleteEntity(IResolverContext ctx, string typeName);
    }

    public class OntologyMutationResolver : IOntologyMutationResolver
    {
        public Task<string> ResolveResponseType(IResolverContext ctx)
        {
            return Task.FromResult("ok");
        }

        public Task<Entity> ResolveResponseDetails(IResolverContext ctx)
        {
            return Task.FromResult(ctx.Parent<Entity>());
        }

        public Task<Entity> CreateEntity(IResolverContext ctx, string typeName)
        {
            return new MutationCreateResolver(ctx).CreateEntity(ctx, typeName);
        }

        public Task<Entity> UpdateEntity(IResolverContext ctx, string typeName)
        {
            return new MutationUpdateResolver(ctx).UpdateEntityAsync(ctx, typeName);
        }

        public Task<Entity> DeleteEntity(IResolverContext ctx, string typeName)
        {
            return Task.FromResult(new MutationDeleteResolver(ctx).DeleteEntity(ctx, typeName));
        }
    }
}
