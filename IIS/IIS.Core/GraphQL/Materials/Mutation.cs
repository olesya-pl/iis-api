using System.Linq;
using System.Threading.Tasks;
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
            [GraphQLNonNullType] MaterialInput input)
        {
            Core.Materials.Material inputMaterial = input.ToDomain();
            await materialService.SaveAsync(inputMaterial, input.ParentId, input?.Metadata?.Features?.Nodes?.ToList());
            Core.Materials.Material material = await materialProvider.GetMaterialAsync(inputMaterial.Id);
            return material.ToView();
        }
    }
}
