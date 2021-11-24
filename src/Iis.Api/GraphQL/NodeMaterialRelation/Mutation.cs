using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using HotChocolate.Resolvers;
using Iis.DbLayer.Repositories;
using Iis.Services;
using IIS.Core.GraphQL.Materials;
using IIS.Services.Contracts.Interfaces;

namespace IIS.Core.GraphQL.NodeMaterialRelation
{
    public class Mutation
    {
        public async Task<Material[]> CreateNodeMaterialRelation(
            IResolverContext ctx,
            [Service] NodeMaterialRelationService<IIISUnitOfWork> relationService,
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [GraphQLNonNullType] NodeMaterialRelationInput input)
        {
            var tokenPayload = ctx.GetToken();
            var materialsSet = input.MaterialIds.ToHashSet();
            var nodesSet = input.NodeIds.ToHashSet();
            await relationService.CreateMultipleRelations(nodesSet, materialsSet, tokenPayload.User.UserName, input.NodeLinkType);
            var material = await materialProvider.GetMaterialsByIdsAsync(materialsSet, tokenPayload.User);
            return mapper.Map<Material[]>(material);
        }

        public async Task<CreateRelationsResponse> CreateMultipleNodeMaterialRelations(
            IResolverContext ctx,
            [Service] NodeMaterialRelationService<IIISUnitOfWork> relationService,
            [GraphQLNonNullType] MultipleNodeMaterialRelationInput input)
        {
            var tokenPayload = ctx.GetToken();
            await relationService.CreateMultipleRelations(tokenPayload.UserId, input.Query, input.NodeId, tokenPayload.User.UserName);
            return new CreateRelationsResponse
            {
                Success = true
            };
        }

        public async Task<Material> DeleteNodeMaterialRelation(
            IResolverContext ctx,
            [Service] NodeMaterialRelationService<IIISUnitOfWork> relationService,
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [GraphQLNonNullType] DeleteNodeMaterialRelationInput input)
        {
            var tokenPayload = ctx.GetToken();
            await relationService.Delete(mapper.Map<Iis.Services.NodeMaterialRelation>(input), tokenPayload.User.UserName);
            var material = await materialProvider.GetMaterialAsync(input.MaterialId, tokenPayload.User);
            return mapper.Map<Material>(material);
        }
    }
}
