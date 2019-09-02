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
        public async Task<string> CreateMaterial([Service] IMaterialService materialService, [GraphQLNonNullType] MaterialInput input)
        {
            await materialService.SaveAsync(Map(input), input.ParentId);
            return "ok";
        }

        private Core.Materials.Material Map(MaterialInput input)
        {
            var result = new Core.Materials.Material(Guid.NewGuid(), JObject.Parse(input.Data), input.Type, input.Source);
            if (input.FileId.HasValue)
                result.File = new FileInfo(input.FileId.Value);
            return result;
        }
    }
}
