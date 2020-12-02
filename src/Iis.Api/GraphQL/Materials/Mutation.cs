using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using HotChocolate.Resolvers;
using IIS.Core.Materials;

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
            var tokenPayload = ctx.ContextData["token"] as TokenPayload;
            var material = await materialService.UpdateMaterialAsync(input, tokenPayload.UserId, tokenPayload.User.UserName);
            return mapper.Map<Material>(material);
        }
    }
}
