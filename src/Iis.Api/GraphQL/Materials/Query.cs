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
using Iis.Services.Contracts.Elastic;
using Newtonsoft.Json.Linq;
using IIS.Services.Contracts.Materials;
using Iis.Domain.Materials;

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
            var filteredItems = ChangeAssigneeFiltered(filter?.FilteredItems ?? new List<Property>(), tokenPayload.UserId);
            var cherryPickedItems = filter?.CherryPickedItems ?? new List<string>();

            if (searchByImageInput != null && searchByImageInput.HasConditions)
            {
                var content = Convert.FromBase64String(searchByImageInput.Content);
                var result = await materialProvider
                    .GetMaterialsByImageAsync(tokenPayload.UserId, pageParam, searchByImageInput.Name, content);
                var mapped = result.Materials.Select(m => mapper.Map<Material>(m)).ToList();
                return (mapped, result.Aggregations, result.Count);
            }

            var relationsState = ParseRelationsState(filter.RelationsState);
            MaterialsDto materialsResult = searchByRelation != null && searchByRelation.HasConditions ?
                await materialProvider.GetMaterialsCommonForEntitiesAsync(
                    tokenPayload.UserId,
                    searchByRelation.NodeIdentityList,
                    searchByRelation.IncludeDescendants,
                    filterQuery,
                    pageParam,
                    sortingParam) :
                await materialProvider.GetMaterialsAsync(
                    tokenPayload.UserId,
                    filterQuery,
                    relationsState,
                    filteredItems,
                    cherryPickedItems,
                    pageParam,
                    sortingParam);

            ChangeAssigneeHighlight(materialsResult.Highlights, tokenPayload.UserId);
            ChangeAssigneeAggregations(materialsResult.Aggregations, tokenPayload.UserId);
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
            var materialsResult = await materialProvider.GetMaterialsByNodeIdAndRelatedEntities(nodeId);

            var materials = materialsResult.Materials.Select(m => mapper.Map<Material>(m)).ToList();
            return (materials, materialsResult.Count);
        }

        [GraphQLType(typeof(MaterialCollection))]
        public async Task<(IEnumerable<Material> materials, int totalCount)> GetRelatedMaterialsByEventId(
           [Service] IMaterialProvider materialProvider,
           [Service] IMapper mapper,
           Guid nodeId)
        {
            var materialsResult = await materialProvider.GetMaterialsByNodeId(nodeId);

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

        private static void ChangeAssigneeAggregations(Dictionary<string, AggregationItem> aggregations, Guid userId)
        {
            var item = aggregations.GetValueOrDefault(MaterialAliases.Assignees.Alias);
            if (item == null) return;

            item.Buckets = item.Buckets.Where(_ => _.Key == userId.ToString()).ToArray();
            if (item.Buckets.Length == 0) return;

            item.Buckets[0].Key = MaterialAliases.Assignees.AliasForSingleItem;
        }

        private static IReadOnlyCollection<Property> ChangeAssigneeFiltered(IReadOnlyCollection<Property> items, Guid userId)
        {
            var result = items.Select(_ => new Property(_.Name, _.Value)).ToArray();
            var assigneeProperty = result.FirstOrDefault(_ => _.Name == MaterialAliases.Assignees.Alias);
            if (assigneeProperty?.Value == MaterialAliases.Assignees.AliasForSingleItem)
            {
                assigneeProperty.Value = userId.ToString();
            }

            return result;
        }

        private static void ChangeAssigneeHighlight(Dictionary<Guid, SearchResultItem> searchResultItems, Guid userId)
        {
            foreach (var item in searchResultItems.Values)
            {
                if (item.Highlight == null) continue;
                var highlight = (JObject)item.Highlight;

                if (highlight.ContainsKey(MaterialAliases.Assignees.Path))
                {
                    highlight.Remove(MaterialAliases.Assignees.Path);
                }

                if (highlight.ContainsKey(MaterialAliases.Assignees.Alias))
                {
                    var value = highlight.GetValue(MaterialAliases.Assignees.Alias).ToString()
                        .Replace(userId.ToString(), MaterialAliases.Assignees.AliasForSingleItem, StringComparison.OrdinalIgnoreCase);
                    highlight[MaterialAliases.Assignees.Alias] = JToken.Parse(value);
                }
            }
        }

        private RelationsState? ParseRelationsState(string relationsState) => !string.IsNullOrWhiteSpace(relationsState)
                && Enum.TryParse<RelationsState>(relationsState.Trim(), true, out var value)
                ? value
                : default(RelationsState?);
    }
}