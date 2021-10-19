using System;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using IIS.Core.Materials;
using System.Linq;
using Iis.Api.GraphQL.Materials;
using IIS.Services.Contracts.Interfaces;

namespace IIS.Core.GraphQL.Materials
{
    public class Mutation
    {
        public async Task<Material> UpdateMaterial(
            IResolverContext ctx,
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [Service] IMaterialProvider materialProvider,
            [GraphQLNonNullType] MaterialUpdateInput input)
        {
            var tokenPayload = ctx.GetToken();
            var material = input.HasValue()
                ? await materialService.UpdateMaterialAsync(input, tokenPayload.User)
                : await materialProvider.GetMaterialFromElasticAsync(input.Id, tokenPayload.User);

            return mapper.Map<Material>(material);
        }

        public async Task<Material> ChangeMaterialAccessLevel(
            IResolverContext context,
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid materialId,
            [GraphQLType(typeof(int))] int newAccessLevel)
        {
            var tokenPayload = context.GetToken();

            var material = await materialService.ChangeMaterialAccessLevel(materialId, newAccessLevel, tokenPayload.User);

            return mapper.Map<Material>(material);
        }

        public async Task<AssignMaterialsOperatorResult> AssignMaterialOperator(IResolverContext context,
            [Service] IMaterialService materialService,
            AssignMaterialOperatorInput input)
        {
            var tokenPayload = context.GetToken();
            var materialIds = input.MaterialIds.ToHashSet();
            var assigneeIds = input.AssigneeIds.ToHashSet();

            await materialService.AssignMaterialOperatorAsync(materialIds, assigneeIds, tokenPayload.User);

            return new AssignMaterialsOperatorResult
            {
                IsSuccess = true
            };
        }

        public async Task<ChangeMaterialEditorResult> AssignMaterialEditor(IResolverContext context,
            [Service] IMaterialService materialService,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid materialId)
        {
            var tokenPayload = context.GetToken();
            var result = await materialService.AssignMaterialEditorAsync(materialId, tokenPayload.User);

            return new ChangeMaterialEditorResult
            {
                IsSuccess = result
            };
        }

        public async Task<ChangeMaterialEditorResult> UnassignMaterialEditor(IResolverContext context,
            [Service] IMaterialService materialService,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid materialId)
        {
            var tokenPayload = context.GetToken();
            var result = await materialService.UnassignMaterialEditorAsync(materialId, tokenPayload.User);

            return new ChangeMaterialEditorResult
            {
                IsSuccess = result
            };
        }
    }
}