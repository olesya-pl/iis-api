using System.Threading.Tasks;
using HotChocolate.Resolvers;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.Mutations.Resolvers
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
        public async Task<string> ResolveResponseType(IResolverContext ctx) => "ok";

        public async Task<Entity> ResolveResponseDetails(IResolverContext ctx) => ctx.Parent<Entity>();

        public Task<Entity> CreateEntity(IResolverContext ctx, string typeName) =>
            new CreateMutationResolver(ctx).CreateEntity(ctx, typeName);

        public Task<Entity> UpdateEntity(IResolverContext ctx, string typeName) =>
            new UpdateMutationResolver(ctx).UpdateEntity(ctx, typeName);

        public Task<Entity> DeleteEntity(IResolverContext ctx, string typeName) =>
            new DeleteMutationResolver(ctx).DeleteEntity(ctx, typeName);
    }
}
