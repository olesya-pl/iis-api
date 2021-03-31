using System;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using IIS.Core.Materials;
using System.Collections.Generic;

namespace IIS.Core.GraphQL.Materials
{
    public class Mutation
    {
        public async Task<Material> UpdateMaterial(
            IResolverContext ctx,
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] MaterialUpdateInput input)
        {
            var tokenPayload = ctx.ContextData[TokenPayload.TokenPropertyName] as TokenPayload;
            var material = await materialService.UpdateMaterialAsync(input, tokenPayload.User);
            return mapper.Map<Material>(material);
        }

        public async Task<Material> ChangeMaterialAccessLevel(
            IResolverContext context,
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid materialId,
            [GraphQLType(typeof(int))] int newAccessLevel
        )
        {
            var tokenPayload = context.ContextData[TokenPayload.TokenPropertyName] as TokenPayload;

            var material = await materialService.ChangeMaterialAccessLevel(materialId, newAccessLevel, tokenPayload.User);

            return mapper.Map<Material>(material);
        }

        public async Task<AssignMaterialsOperatorResult> AssignMaterialsOperator(IResolverContext context,
            [Service] IMaterialService materialService,
            AssignMaterialsOperatorInput input
            )
        {
            var tokenPayload = context.ContextData[TokenPayload.TokenPropertyName] as TokenPayload;
            await materialService.AssignMaterialsOperatorAsync(new HashSet<Guid>(input.MaterialIds), input.AssigneeId, tokenPayload.User);
            return new AssignMaterialsOperatorResult
            {
                IsSuccess = true
            };
        }
    }
}
