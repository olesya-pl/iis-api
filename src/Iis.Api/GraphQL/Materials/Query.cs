using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using IIS.Core.GraphQL.Common;
using Iis.Interfaces.Elastic;
using Iis.Api.GraphQL.Common;
using Iis.Api.GraphQL.Entities;
using HotChocolate.Resolvers;
using Iis.Services.Contracts.Params;
using IIS.Services.Contracts.Interfaces;

namespace IIS.Core.GraphQL.Materials
{
    public class Query
    {

        [GraphQLType(typeof(AggregatedMaterialCollection))]
        public async Task<(IEnumerable<Material> materials, Dictionary<string, AggregationItem> aggregations, int totalCount)> GetMaterials(
            IResolverContext ctx,
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [GraphQLNonNullType] PaginationInput pagination,
            MaterialsFilterInput filter,
            SortingInput sorting,
            SearchByImageInput searchByImageInput,
            SearchByRelationInput searchByRelation = null)
        {
            var tokenPayload = ctx.GetToken();

            var filterQuery = filter?.Suggestion;
            var sortingParam = mapper.Map<SortingParams>(sorting) ?? SortingParams.Default;
            var pageParam = new PaginationParams(pagination.Page, pagination.PageSize);
            var filteredItems = filter?.FilteredItems ?? new List<Property>();
            var cherryPickedItems = filter?.CherryPickedItems ?? new List<string>();

            if (searchByImageInput != null && searchByImageInput.HasConditions)
            {
                var content = Convert.FromBase64String(searchByImageInput.Content);
                var result = await materialProvider
                    .GetMaterialsByImageAsync(tokenPayload.UserId, pageParam, searchByImageInput.Name, content);
                var mapped = result.Materials.Select(m => mapper.Map<Material>(m)).ToList();
                return (mapped, result.Aggregations, result.Count);
            }

            if(searchByRelation != null && searchByRelation.HasConditions)
            {
                var materialsResults = await materialProvider.GetMaterialsCommonForEntitiesAsync(
                    tokenPayload.UserId,
                    searchByRelation.NodeIdentityList,
                    searchByRelation.IncludeDescendants, 
                    filterQuery,
                    pageParam,
                    sortingParam);

                var mapped = materialsResults.Materials
                                .Select(m => mapper.Map<Material>(m))
                                .ToList();

                return (mapped, materialsResults.Aggregations, materialsResults.Count);
            }

            var materialsResult = await materialProvider
                .GetMaterialsAsync(tokenPayload.UserId, filterQuery, filteredItems, cherryPickedItems, pageParam, sortingParam);

            var materials = materialsResult.Materials.Select(m => mapper.Map<Material>(m)).ToList();
            MapHighlights(materials, materialsResult.Highlights);

            return (materials, materialsResult.Aggregations, materialsResult.Count);
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
            IResolverContext ctx,
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            Guid materialId)
        {
            var tokenPayload = ctx.GetToken();
            var material = await materialProvider.GetMaterialAsync(materialId, tokenPayload.User);
            var res = mapper.Map<Material>(material);

            var locationDtoList = await materialProvider.GetLocationHistoriesAsync(materialId);

            res.CoordinateList = locationDtoList.Select(e => 
                new GeoCoordinate
                {
                    Label = "material",
                    Lat = e.Lat,
                    Long = e.Long,
                    PropertyName = "material.location"
                }
            ).ToArray();

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
            IResolverContext ctx,
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [GraphQLNonNullType] Guid materialId,
            [GraphQLNonNullType] PaginationInput pagination,
            [GraphQLNonNullType] SortingInput sorting)
        {
            var pageParam = new PaginationParams(pagination.Page, pagination.PageSize);
            var sortingParams = new SortingParams(sorting.ColumnName, sorting.Order);

            var tokenPayload = ctx.GetToken();
            var materialsResult = await materialProvider.GetMaterialsLikeThisAsync(tokenPayload.UserId, materialId, pageParam, sortingParams);

            var materials = materialsResult.Materials
                                .Select(m => mapper.Map<Material>(m))
                                .ToList();

            return (materials, materialsResult.Count);
        }
    }
}
