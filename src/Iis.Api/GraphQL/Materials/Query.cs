using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using IIS.Core.GraphQL.Common;
using Iis.Interfaces.Elastic;
using Iis.Api.GraphQL.Common;
using Iis.Api.GraphQL.Entities;
using HotChocolate.Resolvers;
using IIS.Core.GraphQL.Users;
using IIS.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Elastic;
using Iis.Services.Contracts.Dtos;
using Newtonsoft.Json.Linq;
using IIS.Services.Contracts.Materials;
using Iis.Domain.Materials;
using Iis.Interfaces.Common;
using Iis.Interfaces.SecurityLevels;

namespace IIS.Core.GraphQL.Materials
{
    public class Query
    {
        private static readonly List<User> DefaultAssignees = new List<User>();
        private static readonly IdTitle DefaultCaller = new IdTitle();
        private static readonly List<Material>DefaultChildren = new List<Material>();
        private static readonly string DefaultCode = string.Empty;
        private static readonly MaterialSign DefaultCompleteness = new MaterialSign();
        private static readonly string DefaultContent = string.Empty;
        private static readonly string DefaultCoordinates = string.Empty;
        private static readonly List<Data> DefaultData = new List<Data>();
        private static readonly User DefaultEditor = new User();
        private static readonly List<JObject> DefaultEvents = new List<JObject>();
        private static readonly List<JObject> DefaultFeatures = new List<JObject>();
        private static readonly string DefaultFrom = string.Empty;
        private static readonly JObject DefaultHighlight = new JObject();
        private static readonly Guid DefaultId = Guid.Empty;
        private static readonly MaterialSign DefaultImportance = new MaterialSign();
        private static readonly JObject DefaultMetadata = new JObject();
        private static readonly List<string> DefaultObjects = new List<string>();
        private static readonly IdTitle DefaultReceiver = new IdTitle();
        private static readonly MaterialSign DefaultRelevance = new MaterialSign();
        private static readonly MaterialSign DefaultReliability = new MaterialSign();
        private static readonly string DefaultSource = string.Empty;
        private static readonly List<string> DefaultStates = new List<string>();
        private static readonly List<string> DefaultTags = new List<string>();
        private static readonly string DefaultTitle = string.Empty;
        private static readonly List<JObject> DefaultTranscriptions = new List<JObject>();
        private static readonly string DefaultType = string.Empty;
        private static readonly int DefaultAccessLevel = default(int);
        private static readonly List<GeoCoordinate> DefaultCoordinateList = new List<GeoCoordinate>();
        private static readonly string DefaultCreatedDate = string.Empty;
        private static readonly string DefaultUpdatedAt = string.Empty;
        private static readonly JObject DefaultObjectsOfStudy = new JObject();
        private static readonly List<RelatedObject> DefaultRelatedSignCollection = new List<RelatedObject>();
        private static readonly List<RelatedObjectOfStudy> DefaultRelatedObjectCollection = new List<RelatedObjectOfStudy>();
        private static readonly List<MaterialSecurityLevel> DefaultSecurityLevels = new List<MaterialSecurityLevel>();
        private static readonly Guid DefaultFileId = Guid.Empty;
        private static readonly DateTime DefaultReceivingDate = DateTime.Now;
        private static readonly MaterialSign DefaultProcessedStatus = new MaterialSign();
        
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
            var createdDateRange = new DateRange(filter?.UploadDateRange?.From, filter?.UploadDateRange?.To);

            if (searchByImageInput != null && searchByImageInput.HasConditions)
            {
                var content = Convert.FromBase64String(searchByImageInput.Content);
                var result = await materialProvider
                    .GetMaterialsByImageAsync(tokenPayload.UserId, pageParam, searchByImageInput.Name, content);
                var mapped = result.Materials.Select(m => mapper.Map<Material>(m)).ToList();
                return (mapped, result.Aggregations, result.Count);
            }

            var relationsState = ParseRelationsState(filter.RelationsState);
            MaterialsDto materialsResult = searchByRelation != null && searchByRelation.HasConditions && cherryPickedItems.Count == 0 ?
                await materialProvider.GetMaterialsCommonForEntitiesAsync(
                    tokenPayload.UserId,
                    searchByRelation.NodeIdentityList,
                    searchByRelation.IncludeDescendants,
                    filterQuery,
                    createdDateRange,
                    pageParam,
                    sortingParam) :
                await materialProvider.GetMaterialsAsync(
                    tokenPayload.UserId,
                    filterQuery,
                    relationsState,
                    filteredItems,
                    cherryPickedItems,
                    createdDateRange,
                    pageParam,
                    sortingParam);

            ChangeAssigneeHighlight(materialsResult.Highlights, tokenPayload.UserId);
            ChangeAssigneeAggregations(materialsResult.Aggregations, tokenPayload.UserId);
            var materials = materialsResult.Materials.Select(m => mapper.Map<Material>(m)).ToList();
            MapHighlights(materials, materialsResult.Highlights);
            return (materials, materialsResult.Aggregations, materialsResult.Count);
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

            res.CoordinateList = MapLocationsToGeoCoordinates(locationDtoList);

            return res;
        }

        public async Task<Material> GetNextAssignedMaterial(
            IResolverContext context,
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper)
        {
            var tokenPayload = context.GetToken();

            var material = await materialProvider.GetNextAssignedMaterialForUserAsync(tokenPayload.User, context.RequestAborted);

            var result = mapper.Map<Material>(material);

            var locationDtoList = await materialProvider.GetLocationHistoriesAsync(result.Id);

            result.CoordinateList = MapLocationsToGeoCoordinates(locationDtoList);

            return result;
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
            IResolverContext ctx,
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [Service] ISecurityLevelChecker securityLevelChecker,
            Guid nodeId)
        {
            var tokenPayload = ctx.GetToken();

            var materialCollectionResult = await materialProvider.GetMaterialsByNodeIdAsync(nodeId, tokenPayload.UserId, ctx.RequestAborted);

            var mappedMaterialCollection = mapper.Map<Material[]>(materialCollectionResult.Items);
            
            CheckIsAllowedRelatedMaterialsForUser(mappedMaterialCollection, tokenPayload, securityLevelChecker);

            return (mappedMaterialCollection, materialCollectionResult.Count);
        }

        public async Task<List<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(
            IResolverContext context,
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            Guid nodeId)
        {
            var tokenPayload = context.GetToken();
            var items = await materialProvider.CountMaterialsByTypeAndNodeAsync(nodeId, tokenPayload.UserId, context.RequestAborted);

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
                    var value = highlight.GetValue(MaterialAliases.Assignees.Alias, StringComparison.OrdinalIgnoreCase).ToString()
                        .Replace(userId.ToString(), MaterialAliases.Assignees.AliasForSingleItem, StringComparison.OrdinalIgnoreCase);
                    highlight[MaterialAliases.Assignees.Alias] = JToken.Parse(value);
                }
            }
        }

        private static IReadOnlyCollection<GeoCoordinate> MapLocationsToGeoCoordinates(IReadOnlyCollection<LocationHistoryDto> locationHistories)
        {
            return locationHistories.Select(e =>
                new GeoCoordinate
                {
                    Label = "material",
                    Lat = e.Lat,
                    Long = e.Long,
                    PropertyName = "material.location"
                }).ToArray();
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

        private static RelationsState? ParseRelationsState(string relationsState) => !string.IsNullOrWhiteSpace(relationsState)
                && Enum.TryParse<RelationsState>(relationsState.Trim(), true, out var value)
                ? value
                : default(RelationsState?);

        private static void CheckIsAllowedRelatedMaterialsForUser(IEnumerable<Material> mappedMaterialCollection,
            TokenPayload tokenPayload, ISecurityLevelChecker securityLevelChecker)
        {
            foreach (var material in mappedMaterialCollection)
            {
                var materialSecurityLevelIndexes = material.SecurityLevels.Select(_ => _.UniqueIndex).ToList();
                
                material.AccessAllowed = securityLevelChecker.AccessGranted(tokenPayload.User.SecurityLevelsIndexes, materialSecurityLevelIndexes);
                
                if (material.AccessAllowed) continue;
                
                material.Assignees = DefaultAssignees;
                material.Caller = DefaultCaller;
                material.Children = DefaultChildren;
                material.Code = DefaultCode;
                material.Completeness = DefaultCompleteness;
                material.Content = DefaultContent;
                material.Coordinates = DefaultCoordinates;
                material.Data = DefaultData;
                material.Editor = DefaultEditor;
                material.Events = DefaultEvents;
                material.Features = DefaultFeatures;
                material.From = DefaultFrom;
                material.Highlight = DefaultHighlight;
                material.Id = DefaultId;
                material.Importance = DefaultImportance;
                material.Metadata = DefaultMetadata;
                material.Objects = DefaultObjects;
                material.Receiver = DefaultReceiver;
                material.Relevance = DefaultRelevance;
                material.Reliability = DefaultReliability;
                material.Source = DefaultSource;
                material.States = DefaultStates;
                material.Tags = DefaultTags;
                material.Title = DefaultTitle;
                material.Transcriptions = DefaultTranscriptions;
                material.Type = DefaultType;
                material.AccessLevel = DefaultAccessLevel;
                material.CoordinateList = DefaultCoordinateList;
                material.CreatedDate = DefaultCreatedDate;
                material.UpdatedAt = DefaultUpdatedAt;
                material.ObjectsOfStudy = DefaultObjectsOfStudy;
                material.RelatedSignCollection = DefaultRelatedSignCollection;
                material.RelatedObjectCollection = DefaultRelatedObjectCollection;
                material.SecurityLevels = DefaultSecurityLevels;
                material.FileId = DefaultFileId;
                material.ReceivingDate = DefaultReceivingDate;
                material.ProcessedStatus = DefaultProcessedStatus;
            }
        }

    }
}