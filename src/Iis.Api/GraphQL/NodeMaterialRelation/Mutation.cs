using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using IIS.Core.GraphQL.Materials;
using IIS.Core.Materials;
using IIS.Core.NodeMaterialRelation;

namespace IIS.Core.GraphQL.NodeMaterialRelation
{
    public class Mutation
    {
        public async Task<Material> CreateNodeMaterialRelation(
            [Service] NodeMaterialRelationService relationService,
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [GraphQLNonNullType] NodeMaterialRelationInput input)
        {
            await relationService.Create(mapper.Map<Core.NodeMaterialRelation.NodeMaterialRelation>(input));
            var material = await materialProvider.GetMaterialAsync(input.MaterialId);
            return mapper.Map<Material>(material);
        }
    }
}

