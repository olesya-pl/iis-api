using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using HotChocolate.Resolvers;
using IIS.Core.GraphQL.Materials;
using IIS.Core.Materials;
using IIS.Core.NodeMaterialRelation;

namespace IIS.Core.GraphQL.NodeMaterialRelation
{
    public class Mutation
    {
        public async Task<Material> CreateNodeMaterialRelation(
            IResolverContext ctx,
            [Service] NodeMaterialRelationService relationService,
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [GraphQLNonNullType] NodeMaterialRelationInput input)
        {
            var tokenPayload = ctx.ContextData["token"] as TokenPayload;
            await relationService.Create(mapper.Map<Core.NodeMaterialRelation.NodeMaterialRelation>(input), tokenPayload.User.UserName);
            var material = await materialProvider.GetMaterialAsync(input.MaterialId, tokenPayload.UserId);
            return mapper.Map<Material>(material);
        }

        public async Task<Material> DeleteNodeMaterialRelation(
            IResolverContext ctx,
            [Service] NodeMaterialRelationService relationService,
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [GraphQLNonNullType] NodeMaterialRelationInput input)
        {
            var tokenPayload = ctx.ContextData["token"] as TokenPayload;
            await relationService.Delete(mapper.Map<Core.NodeMaterialRelation.NodeMaterialRelation>(input), tokenPayload.User.UserName);
            var material = await materialProvider.GetMaterialAsync(input.MaterialId, tokenPayload.UserId);
            return mapper.Map<Material>(material);
        }
    }
}

