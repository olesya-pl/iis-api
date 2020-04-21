using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using IIS.Core.NodeMaterialRelation;

namespace IIS.Core.GraphQL.NodeMaterialRelation
{
    public class Mutation
    {
        public async Task<NodeMaterialRelation> CreateNodeMaterialRelation(
            [Service] NodeMaterialRelationService relationService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] NodeMaterialRelationInput input)
        {
            await relationService.Create(mapper.Map<Core.NodeMaterialRelation.NodeMaterialRelation>(input));
            return mapper.Map<NodeMaterialRelation>(input);
        }
    }
}

