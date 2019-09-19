using System;
using System.Threading.Tasks;
using HotChocolate;
using IIS.Core.Files;
using IIS.Core.Materials;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Materials
{
    public class Mutation
    {
        public async Task<Material> CreateMaterial(
            [Service] IMaterialProvider materialProvider,
            [Service] IMaterialService materialService,
            [GraphQLNonNullType] MaterialInput input)
        {
            var inputMaterial = input.ToDomain();
            await materialService.SaveAsync(inputMaterial, input.ParentId);
            var material = await materialProvider.GetMaterialAsync(inputMaterial.Id);
            return material.ToView();
        }
    }
}
