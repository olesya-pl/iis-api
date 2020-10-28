using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using IIS.Core.Materials;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities.InputTypes;
using Iis.Interfaces.Elastic;
using Iis.Api.GraphQL.Common;

namespace IIS.Core.GraphQL.Materials
{
    public class Query
    {

        [GraphQLType(typeof(MaterialCollection))]
        public async Task<(IEnumerable<Material> materials, int totalCount)> GetMaterials(
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [GraphQLNonNullType] PaginationInput pagination,
            FilterInput filter,
            SortingInput sorting,
            SearchByImageInput searchByImageInput,
            IEnumerable<Guid> relationNodeIdList = null,
            IEnumerable<string> types = null)
        {
            var filterQuery = filter?.Suggestion ?? filter?.SearchQuery;

            if (searchByImageInput != null)
            {
                var content = Convert.FromBase64String(searchByImageInput.Content);
                var result = await materialProvider
                    .GetMaterialsByImageAsync(pagination.PageSize, pagination.Offset(), searchByImageInput.Name, content.ToArray());
                var mapped = result.Materials.Select(m => mapper.Map<Material>(m)).ToList();
                return (mapped, result.Count);
            }

            if(relationNodeIdList != null && relationNodeIdList.Any())
            {
                var materialsResults = await materialProvider.GetMaterialsCommonForEntityAndDescendantsAsync(relationNodeIdList, pagination.PageSize, pagination.Offset());

                var mapped = materialsResults.Materials
                                .Select(m => mapper.Map<Material>(m))
                                .ToList();

                return (mapped, materialsResults.Count);
            }

            var materialsResult = await materialProvider
                .GetMaterialsAsync(pagination.PageSize, pagination.Offset(), filterQuery, types,
                    sorting?.ColumnName, sorting?.Order);

            var materials = materialsResult.Materials.Select(m => mapper.Map<Material>(m)).ToList();
            MapHighlights(materials, materialsResult.Highlights);

            return (materials, materialsResult.Count);
        }

        private static void MapHighlights(List<Material> materials, Dictionary<Guid, SearchResultItem> materialsResult)
        {
            foreach (var material in materials)
            {
                if (materialsResult.ContainsKey(material.Id))
                {
                    material.Highlight = materialsResult[material.Id].Highlight;
                }
            }
        }

        public async Task<Material> GetMaterial(
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            Guid materialId)
        {
            var material = await materialProvider.GetMaterialAsync(materialId);
            var res = mapper.Map<Material>(material);
            return res;
        }

        public Task<IEnumerable<MaterialSignFull>> GetImportanceSigns([Service] IMaterialProvider materialProvider, [Service] IMapper mapper)
        {
            return Task.FromResult(materialProvider.GetMaterialSigns("Importance").Select(ms => mapper.Map<MaterialSignFull>(ms)));
        }

        public Task<IEnumerable<MaterialSignFull>> GetReliabilitySigns([Service] IMaterialProvider materialProvider, [Service] IMapper mapper)
        {
            return Task.FromResult(materialProvider.GetMaterialSigns("Reliability").Select(ms => mapper.Map<MaterialSignFull>(ms)));
        }

        public Task<IEnumerable<MaterialSignFull>> GetRelevanceSigns([Service] IMaterialProvider materialProvider, [Service] IMapper mapper)
        {
            return Task.FromResult(materialProvider.GetMaterialSigns("Relevance").Select(ms => mapper.Map<MaterialSignFull>(ms)));
        }

        public Task<IEnumerable<MaterialSignFull>> GetCompletenessSigns([Service] IMaterialProvider materialProvider, [Service] IMapper mapper)
        {
            return Task.FromResult(materialProvider.GetMaterialSigns("Completeness").Select(ms => mapper.Map<MaterialSignFull>(ms)));
        }

        public Task<IEnumerable<MaterialSignFull>> GetSourceReliabilitySigns([Service] IMaterialProvider materialProvider, [Service] IMapper mapper)
        {
            return Task.FromResult(materialProvider.GetMaterialSigns("SourceReliability").Select(ms => mapper.Map<MaterialSignFull>(ms)));
        }

        public Task<IEnumerable<MaterialSignFull>> GetProcessedStatusSigns([Service] IMaterialProvider materialProvider, [Service] IMapper mapper)
        {
            return Task.FromResult(materialProvider.GetMaterialSigns("ProcessedStatus").Select(ms => mapper.Map<MaterialSignFull>(ms)));
        }

        public Task<IEnumerable<MaterialSignFull>> GetSessionPrioritySigns([Service] IMaterialProvider materialProvider, [Service] IMapper mapper)
        {
            return Task.FromResult(materialProvider.GetMaterialSigns("SessionPriority").Select(ms => mapper.Map<MaterialSignFull>(ms)));
        }

        [GraphQLType(typeof(MaterialCollection))]
        public async Task<(IEnumerable<Material> materials, int totalCount)> GetRelatedMaterialsByNodeId(
           [Service] IMaterialProvider materialProvider,
           [Service] IMapper mapper,
           Guid nodeId)
        {
            var materialsResult = await materialProvider.GetMaterialsByNodeIdQuery(nodeId);

            var materials = materialsResult.Materials.Select(m => mapper.Map<Material>(m)).ToList();
            return (materials, materialsResult.Count);
        }

        public async Task<List<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            Guid nodeId)
        {
            var items = await materialProvider.CountMaterialsByTypeAndNodeAsync(nodeId);
            return mapper.Map<List<MaterialsCountByType>>(items);
        }

        public async Task<(IEnumerable<Material> materials, int totalCount)> GetMaterialsByAssigneeId(
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            Guid assigneeId)
        {
            var materialsResult = await materialProvider.GetMaterialsByAssigneeIdAsync(assigneeId);

            var materials = materialsResult.Materials.Select(m => mapper.Map<Material>(m)).ToList();
            return (materials, materialsResult.Count);
        }

        public async Task<(IEnumerable<Material> materials, int totalCout)> GetMaterialsLikeThis(
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [GraphQLNonNullType] Guid materialId,
            [GraphQLNonNullType] PaginationInput pagination)
        {
            var materialsResult = await materialProvider.GetMaterialsLikeThisAsync(materialId, pagination.PageSize, pagination.Offset());

            var materials = materialsResult.Materials
                                .Select(m => mapper.Map<Material>(m))
                                .ToList();

            return (materials, materialsResult.Count);
        }
    }
}
