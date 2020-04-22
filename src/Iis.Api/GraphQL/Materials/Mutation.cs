using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using IIS.Core.Materials;

namespace IIS.Core.GraphQL.Materials
{
    public class Mutation
    {
        private readonly IMapper _mapper;
        // ReSharper disable once UnusedMember.Global
        public async Task<Material> CreateMaterial(
            [Service] IMaterialProvider materialProvider,
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] MaterialInput input)
        {
            Iis.Domain.Materials.Material inputMaterial = _mapper.Map<Iis.Domain.Materials.Material>(input);
            
            await materialService.SaveAsync(inputMaterial, input?.Metadata?.Features?.Nodes?.ToList());

            Iis.Domain.Materials.Material material = await materialProvider.GetMaterialAsync(inputMaterial.Id);
            
            return mapper.Map<Material>(material);
        }

        public async Task<Material> UpdateMaterial(
            [Service] IMaterialProvider materialProvider,
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] MaterialUpdateInput input)
        {
            var materialEntity = await materialProvider.UpdateMaterial(input);
            await materialService.SaveAsync(materialEntity);

            var materialDomain = await materialProvider.MapAsync(materialEntity);
            return mapper.Map<Material>(materialDomain);
        }
    }
}
