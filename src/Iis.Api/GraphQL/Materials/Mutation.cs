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
            Iis.Domain.Materials.Material inputMaterial = input.ToDomain();
            await materialService.SaveAsync(inputMaterial, input.ParentId, input?.Metadata?.Features?.Nodes?.ToList());
            Iis.Domain.Materials.Material material = await materialProvider.GetMaterialAsync(inputMaterial.Id);
            return mapper.Map<Material>(material);
        }

        public async Task<Material> UpdateMaterial(
            [Service] IMaterialProvider materialProvider,
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] MaterialUpdateInput input)
        {
            var materialEntity = await materialProvider.GetMaterialEntityAsync(input.Id);
            //if (input.Title != null) material.Title = input.Title;
            if (input.ImportanceId != null) materialEntity.ImportanceSignId = input.ImportanceId;
            if (input.ReliabilityId != null) materialEntity.ReliabilitySignId = input.ReliabilityId;
            if (input.RelevanceId != null) materialEntity.RelevanceSignId = input.RelevanceId;
            if (input.CompletenessId != null) materialEntity.CompletenessSignId = input.CompletenessId;
            if (input.SourceReliabilityId != null) materialEntity.SourceReliabilitySignId = input.SourceReliabilityId;
            if (input.Title != null) materialEntity.Title = input.Title;
            if (input.Objects != null )
            await materialService.SaveAsync(materialEntity);
            var materialDomain = await materialProvider.MapAsync(materialEntity);
            return mapper.Map<Material>(materialDomain);
        }
    }
}
