using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using IIS.Core.Materials;
using IIS.Core.Materials.FeatureProcessors;

namespace IIS.Core.GraphQL.Materials
{
    public class Mutation
    {
        public async Task<Material> UpdateMaterial(
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] MaterialUpdateInput input)
        {
            var material = await materialService.UpdateMaterialAsync(input);
            return mapper.Map<Material>(material);
        }
    }
}
