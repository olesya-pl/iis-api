using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using IIS.Core.Materials;

namespace IIS.Core.GraphQL.Materials
{
    public class Mutation
    {
        // ReSharper disable once UnusedMember.Global
        public async Task<Material> CreateMaterial(
            [Service] IMaterialProvider materialProvider,
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] MaterialInput input)
        {
            Iis.Domain.Materials.Material inputMaterial = mapper.Map<Iis.Domain.Materials.Material>(input);

            await materialService.SaveAsync(inputMaterial, input?.Metadata?.Features?.Nodes?.ToList());

            Iis.Domain.Materials.Material material = await materialProvider.GetMaterialAsync(inputMaterial.Id);

            return mapper.Map<Material>(material);
        }

        public async Task<Material> UpdateMaterial(
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] MaterialUpdateInput input)
        {
            var material = await materialService.UpdateMaterial(input);
            return mapper.Map<Material>(material);
        }

        public async Task<Material> AssignMaterialOperator(
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] AssignMaterialOperatorInput input)
        {
            var material = await materialService.AssignMaterialOperatorAsync(input.MaterialId, input.AssigneeId);
            return mapper.Map<Material>(material);
        }
    }
}
