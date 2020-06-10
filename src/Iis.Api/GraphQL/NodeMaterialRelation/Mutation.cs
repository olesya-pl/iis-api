using System;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using Iis.Interfaces.Ontology.Schema;
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
            var parsed = Enum.TryParse<EntityTypeNames>(input.NodeType, true, out var nodeType);
            if (!parsed)
            {
                nodeType = EntityTypeNames.ObjectOfStudy;
            }
            await relationService.Create(mapper.Map<Core.NodeMaterialRelation.NodeMaterialRelation>(input), nodeType);
            var material = await materialProvider.GetMaterialAsync(input.MaterialId);
            return mapper.Map<Material>(material);
        }

        public async Task<Material> DeleteNodeMaterialRelation(
            [Service] NodeMaterialRelationService relationService,
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [GraphQLNonNullType] NodeMaterialRelationInput input)
        {
            await relationService.Delete(mapper.Map<Core.NodeMaterialRelation.NodeMaterialRelation>(input));
            var material = await materialProvider.GetMaterialAsync(input.MaterialId);
            return mapper.Map<Material>(material);
        }
    }
}

