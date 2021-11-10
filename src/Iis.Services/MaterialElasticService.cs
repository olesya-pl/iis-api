using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Enums;
using Iis.Services.Contracts.Params;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Interfaces.Elastic;
using Iis.Elastic;
using Iis.Elastic.Dictionaries;
using Iis.Elastic.SearchQueryExtensions;
using System.Linq;
using Iis.DbLayer.Repositories;
using Iis.DataModel.Materials;
using Iis.DbLayer.MaterialDictionaries;
using Newtonsoft.Json.Linq;
using Iis.DbLayer.Repositories.Helpers;
using Iis.Utility;
using AutoMapper;
using Iis.Interfaces.Ontology.Data;
using Iis.DataModel.ChangeHistory;
using Iis.Utility.Elasticsearch.Documents;
using System.Text.RegularExpressions;
using Iis.DbLayer.MaterialEnum;
using IIS.Repository;
using IIS.Repository.Factories;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Iis.Services
{
    public class MaterialElasticService<TUnitOfWork> : BaseService<TUnitOfWork>, IMaterialElasticService
         where TUnitOfWork : IIISUnitOfWork
    {
        public const int MaterialsBatchSize = 5000;
        public const int MaterialChangesBatchSize = 5000;
        private const string ExclamationMark = "!";

        private static readonly string[] IgnoreDocumentPropertyNames = new[] { "Content" };
        private static readonly string NoneLinkTypeValue = MaterialNodeLinkType.None.ToString();
        private static readonly MaterialIncludeEnum[] IncludeAll = new[]
        {
            MaterialIncludeEnum.WithChildren,
            MaterialIncludeEnum.WithFeatures
        };
        private static IReadOnlyCollection<AggregationField> AggregationsFieldList = new List<AggregationField>
        {
            new AggregationField(MaterialAliases.ProcessedStatus.Path, MaterialAliases.ProcessedStatus.Alias, MaterialAliases.ProcessedStatus.Path),
            new AggregationField(MaterialAliases.Completeness.Path, MaterialAliases.Completeness.Alias, MaterialAliases.Completeness.Path),
            new AggregationField(MaterialAliases.Importance.Path, MaterialAliases.Importance.Alias, MaterialAliases.Importance.Path),
            new AggregationField(MaterialAliases.SessionPriority.Path, MaterialAliases.SessionPriority.Alias, MaterialAliases.SessionPriority.Path),
            new AggregationField(MaterialAliases.Reliability.Path, MaterialAliases.Reliability.Alias, MaterialAliases.Reliability.Path),
            new AggregationField(MaterialAliases.Relevance.Path, MaterialAliases.Relevance.Alias, MaterialAliases.Relevance.Path),
            new AggregationField(MaterialAliases.SourceReliability.Path, MaterialAliases.SourceReliability.Alias, MaterialAliases.SourceReliability.Path),
            new AggregationField(MaterialAliases.Type.Path, MaterialAliases.Type.Alias, MaterialAliases.Type.Path),
            new AggregationField(MaterialAliases.Source.Path, MaterialAliases.Source.Alias, MaterialAliases.Source.Path),
            new AggregationField(MaterialAliases.Assignees.Path, MaterialAliases.Assignees.Alias, MaterialAliases.Assignees.Path),
        };

        private readonly IElasticManager _elasticManager;
        private readonly IElasticState _elasticState;
        private readonly IElasticResponseManagerFactory _elasticResponseManagerFactory;
        private readonly ElasticConfiguration _elasticConfiguration;
        private readonly IMLResponseRepository _mLResponseRepository;
        private readonly IMapper _mapper;
        private readonly IOntologyNodesData _ontologyData;
        private readonly ILogger<MaterialElasticService<TUnitOfWork>> _logger;

        public MaterialElasticService(IElasticManager elasticManager,
            IElasticState elasticState,
            IElasticResponseManagerFactory elasticResponseManagerFactory,
            ElasticConfiguration elasticConfiguration,
            IMLResponseRepository mLResponseRepository,
            IMapper mapper,
            IOntologyNodesData ontologyData,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            ILogger<MaterialElasticService<TUnitOfWork>> logger)
            : base(unitOfWorkFactory)
        {
            _elasticManager = elasticManager;
            _elasticState = elasticState;
            _elasticResponseManagerFactory = elasticResponseManagerFactory;
            _elasticConfiguration = elasticConfiguration;
            _mLResponseRepository = mLResponseRepository;
            _mapper = mapper;
            _ontologyData = ontologyData;
            _logger = logger;
        }

        public bool ShouldReturnNoEntities(string queryExpression)
        {
            return queryExpression?.Trim() == ExclamationMark;
        }

        public async Task<SearchResult> SearchMaterialsByConfiguredFieldsAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default)
        {
            var (from, size) = searchParams.Page.ToElasticPage();

            var queryString = SearchQueryExtension.CreateMaterialsQueryString(
                searchParams.Suggestion,
                ChangeAssigneeFiltered(searchParams.FilteredItems, userId),
                searchParams.CherryPickedItems);

            var query = new ExactQueryBuilder()
                .WithPagination(from, size)
                .WithQueryString(queryString)
                .BuildSearchQuery()
                .WithAggregation(AggregationsFieldList)
                .WithHighlights();

            if (searchParams.Sorting != null)
            {
                var (sortColumn, sortOrder) = MapSortingToElastic(searchParams.Sorting);
                query = query.SetupSorting(sortColumn, sortOrder);
            }

            var elasticResult = await _elasticManager
                .WithUserId(userId)
                .SearchAsync(query.ToString(Formatting.None), _elasticState.MaterialIndexes, ct);

            var searchResult = elasticResult.ToSearchResult();

            foreach (var item in searchResult.Items)
            {
                if (item.Value.Highlight is null) continue;

                var highlight = await _elasticResponseManagerFactory.Create(SearchType.Material)
                    .GenerateHighlightsWithoutDublications(item.Value.SearchResult, item.Value.Highlight);

                item.Value.Highlight = ChangeAssigneeHighlight(highlight, userId);
            }

            if (ItemsCountPossiblyExceedsMaxThreshold(searchResult))
            {
                var countQuery = new ExactQueryBuilder()
                    .WithQueryString(queryString)
                    .BuildCountQuery()
                    .ToString();
                searchResult.Count = await _elasticManager
                    .WithUserId(userId)
                    .CountAsync(countQuery, _elasticState.MaterialIndexes, ct);
            }

            ChangeAssigneeAggregations(searchResult.Aggregations, userId);
            return searchResult;
        }

        private static bool ItemsCountPossiblyExceedsMaxThreshold(SearchResult searchResult)
        {
            return searchResult.Count == ElasticConstants.MaxItemsCount;
        }

        public async Task<SearchResult> BeginSearchByScrollAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default)
        {
            var (from, size) = searchParams.Page.ToElasticPage();

            var queryString = SearchQueryExtension.CreateMaterialsQueryString(
                searchParams.Suggestion,
                searchParams.FilteredItems,
                searchParams.CherryPickedItems);

            var scrollDuration = _elasticConfiguration.ScrollDurationMinutes == default(int)
                ? ElasticConstants.DefaultScrollDurationMinutes
                : _elasticConfiguration.ScrollDurationMinutes;

            var query = new ExactQueryBuilder()
                .WithPagination(from, size)
                .WithQueryString(queryString)
                .BuildSearchQuery();

            var elasticResult = await _elasticManager
                .WithUserId(userId)
                .BeginSearchByScrollAsync(query.ToString(), TimeSpan.FromMinutes(scrollDuration), _elasticState.MaterialIndexes, ct);

            return elasticResult.ToSearchResult();
        }

        public async Task<SearchResult> SearchByScroll(Guid userId, string scrollId)
        {
            var scrollDuration = _elasticConfiguration.ScrollDurationMinutes == default(int)
                ? ElasticConstants.DefaultScrollDurationMinutes
                : _elasticConfiguration.ScrollDurationMinutes;

            var elasticResult = await _elasticManager
                .WithUserId(userId)
                .SearchByScrollAsync(scrollId, TimeSpan.FromMinutes(scrollDuration));
            return elasticResult.ToSearchResult();
        }

        public async Task<MaterialDocument> GetMaterialById(Guid userId, Guid materialId)
        {
            var result = await _elasticManager
                .WithUserId(userId)
                .GetDocumentByIdAsync(_elasticState.MaterialIndexes, materialId.ToString("N"));
            
            if (!result.Items.Any()) return null;

            return MaterialDocument.FromJObject(result.Items.First().SearchResult);
        }

        public async Task<SearchResult> SearchMaterialsAsync(Guid userId,
            SearchParams searchParams,
            IEnumerable<Guid> materialList,
            CancellationToken ct = default)
        {
            var (from, size) = searchParams.Page.ToElasticPage();

            var queryBuilder = new BoolQueryBuilder()
                                .WithMust()
                                .WithPagination(from, size)
                                .WithDocumentList(materialList);

            if (!SearchQueryExtension.IsMatchAll(searchParams.Suggestion))
            {
                queryBuilder.WithExactQuery(searchParams.Suggestion);
            }

            var queryObj = queryBuilder
                            .BuildSearchQuery()
                            .WithHighlights();

            if (searchParams.Sorting != null)
            {
                var (sortColumn, sortOrder) = MapSortingToElastic(searchParams.Sorting);
                queryObj = queryObj.SetupSorting(sortColumn, sortOrder);
            }

            var query = queryObj
                .WithAggregation(AggregationsFieldList)
                .ToString(Formatting.None);

            var elasticResult = await _elasticManager
                .WithUserId(userId)
                .SearchAsync(query, _elasticState.MaterialIndexes, ct);

            var searchResult = elasticResult.ToSearchResult();

            foreach (var item in searchResult.Items)
            {
                if (item.Value.Highlight is null) continue;

                item.Value.Highlight = await _elasticResponseManagerFactory.Create(SearchType.Material)
                 .GenerateHighlightsWithoutDublications(item.Value.SearchResult, item.Value.Highlight);
            }

            return searchResult;
        }

        public async Task<SearchResult> SearchMoreLikeThisAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default)
        {
            var (from, size) = searchParams.Page.ToElasticPage();
            var (sortColumn, sortOrder) = MapSortingToElastic(searchParams.Sorting);

            var queryData = new MoreLikeThisQueryBuilder()
                        .WithPagination(from, size)
                        .WithMaterialId(searchParams.Suggestion)
                        .BuildSearchQuery()
                        .SetupSorting(sortColumn, sortOrder)
                        .ToString(Formatting.None);

            var searchResult = await _elasticManager
                .WithUserId(userId)
                .SearchAsync(queryData, _elasticState.MaterialIndexes, ct);

            return searchResult.ToSearchResult();
        }

        public async Task<SearchResult> SearchByImageVector(Guid userId, IReadOnlyCollection<decimal[]> imageVectorList, PaginationParams page, CancellationToken ct = default)
        {
            var (from, size) = page.ToElasticPage();

            var query = new SearchByImageQueryBuilder(imageVectorList)
                .WithPagination(from, size)
                .BuildSearchQuery()
                .WithAggregation(AggregationsFieldList);

            var searchResult = await _elasticManager
                .WithUserId(userId)
                .SearchAsync(query.ToString(), _elasticState.MaterialIndexes, ct);

            return searchResult.ToSearchResult();
        }

        public Task<int> CountMaterialsByConfiguredFieldsAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default)
        {
            var queryString = SearchQueryExtension.CreateMaterialsQueryString(
                searchParams.Suggestion,
                searchParams.FilteredItems,
                searchParams.CherryPickedItems);

            var pagination = searchParams.Page.ToEFPage();

            var exactQueryBuilder = new ExactQueryBuilder()
                .WithQueryString(queryString)
                .WithLeniency(true)
                .WithPagination(pagination.Skip, pagination.Take)
                .BuildCountQuery();

            return _elasticManager
                .WithUserId(userId)
                .CountAsync(exactQueryBuilder.ToString(), _elasticState.MaterialIndexes, ct);
        }

        public async Task<List<ElasticBulkResponse>> PutAllMaterialsToElasticSearchAsync(CancellationToken cancellationToken = default)
        {
            const int MaxDegreeOfParallelism = 8;
            int materialsCount = await RunWithoutCommitAsync(_ => _.MaterialRepository.GetTotalCountAsync(cancellationToken));
            if (materialsCount == 0)
                return new List<ElasticBulkResponse>();

            var responses = new List<ElasticBulkResponse>(materialsCount);

            for (var batchIndex = 0; batchIndex < (materialsCount / MaterialsBatchSize) + 1; batchIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var sw = new Stopwatch();
                sw.Start();
                _logger.LogInformation("PutAllMaterialsToElasticSearchAsync. Indexing batch {batchIndex}", batchIndex);

                var materialEntities = await RunWithoutCommitAsync(_ => _.MaterialRepository.GetAllAsync(MaterialsBatchSize, batchIndex * MaterialsBatchSize, IncludeAll));
                _logger.LogInformation("PutAllMaterialsToElasticSearchAsync. Obtained materials from Postgre. Elapsed {elapsed}", sw.ElapsedMilliseconds);
                var materialIds = materialEntities
                    .Select(_ => _.Id)
                    .ToArray();
                _logger.LogInformation("PutAllMaterialsToElasticSearchAsync. Selected material ids. Elapsed {elapsed}", sw.ElapsedMilliseconds);
                var mlResponsesList = await _mLResponseRepository.GetAllForMaterialListAsync(materialIds);
                _logger.LogInformation("PutAllMaterialsToElasticSearchAsync. Obtained ML responses from Postgre. Elapsed {elapsed}", sw.ElapsedMilliseconds);
                var mlResponseDictionary = mlResponsesList
                    .GroupBy(p => p.MaterialId)
                    .ToDictionary(k => k.Key, p => p.ToArray());
                _logger.LogInformation("PutAllMaterialsToElasticSearchAsync. Group by material id. Elapsed {elapsed}", sw.ElapsedMilliseconds);
                var materialDocuments = materialEntities
                    .AsParallel()
                    .WithDegreeOfParallelism(MaxDegreeOfParallelism)
                    .Select(p => MapEntityToDocument(p))
                    .Select(p =>
                    {
                        var materialIdList = p.Children
                                                .Select(e => e.Id)
                                                .Union(new[] { p.Id })
                                                .ToArray();

                        var (mlResponses, mlResponsesCount) = GetResponseJsonWithCounter(p.Id, mlResponseDictionary);

                        p.MLResponses = mlResponses;

                        p.ProcessedMlHandlersCount = mlResponsesCount;

                        p.ImageVectors = GetImageVectorList(materialIdList, mlResponseDictionary);

                        return p;
                    })
                    .ToDictionary(_ => _.Id);
                _logger.LogInformation("PutAllMaterialsToElasticSearchAsync. Mapped materials to elastic documents. Elapsed {elapsed}", sw.ElapsedMilliseconds);
                string json = materialDocuments.ConvertToJson();
                _logger.LogInformation("PutAllMaterialsToElasticSearchAsync. Generated json query. Elapsed {elapsed}", sw.ElapsedMilliseconds);
                var response = await _elasticManager.PutDocumentsAsync(_elasticState.MaterialIndexes.FirstOrDefault(), json, false, cancellationToken);
                _logger.LogInformation("PutAllMaterialsToElasticSearchAsync. Persisted materials to elastic. Elapsed {elapsed}", sw.ElapsedMilliseconds);
                responses.AddRange(response);
                sw.Stop();
            }

            return responses;
        }

        public async Task<List<ElasticBulkResponse>> PutAllMaterialChangesToElasticSearchAsync(CancellationToken cancellationToken = default)
        {
            int count = await RunWithoutCommitAsync(_ => _.ChangeHistoryRepository.GetTotalCountAsync(_ => _.Type == ChangeHistoryEntityType.Material
                    || _.Type == ChangeHistoryEntityType.Node && _.PropertyName == ChangeHistoryDocument.MaterialLinkPropertyName, cancellationToken));
            if (count == 0)
                return new List<ElasticBulkResponse>();

            var responses = new List<ElasticBulkResponse>(count);

            for (var batchIndex = 0; batchIndex < (count / MaterialChangesBatchSize) + 1; batchIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var entities = await RunWithoutCommitAsync(_ => _.ChangeHistoryRepository.GetAllAsync(MaterialChangesBatchSize,
                    batchIndex * MaterialChangesBatchSize,
                    _ => _.Type == ChangeHistoryEntityType.Material
                        || _.Type == ChangeHistoryEntityType.Node && _.PropertyName == ChangeHistoryDocument.MaterialLinkPropertyName,
                    cancellationToken));
                var response = await PutMaterialChangesToElasticSearchAsync(entities, false, cancellationToken);

                responses.AddRange(response);
            }

            return responses;
        }

        public async Task<List<ElasticBulkResponse>> PutMaterialChangesToElasticSearchAsync(
            IReadOnlyCollection<ChangeHistoryEntity> changes,
            bool waitForIndexing = false,
            CancellationToken cancellationToken = default)
        {
            var userNames = changes
                .Select(_ => _.UserName)
                .Distinct()
                .ToHashSet();
            var userRolesDictionary = await RunWithoutCommitAsync(_ => _.UserRepository.GetRolesByUserNamesDictionaryAsync(userNames, cancellationToken));
            var documents = changes
                .Select(_ => _mapper.Map<ChangeHistoryDocument>(_))
                .Select(_ =>
                {
                    if (IgnoreDocumentPropertyNames.Contains(_.PropertyName))
                    {
                        _.OldValue = null;
                        _.NewValue = null;
                    }

                    if (string.IsNullOrWhiteSpace(_.UserName)
                        || !userRolesDictionary.TryGetValue(_.UserName, out var roles))
                        return _;

                    _.Roles = roles.Select(_ => new Role
                    {
                        Id = _.Id,
                        Name = _.Name
                    }).ToArray();

                    return _;
                })
                .ToDictionary(_ => _.Id);
            string json = documents.ConvertToJson();
            var response = await _elasticManager.PutDocumentsAsync(_elasticState.ChangeHistoryIndexes.FirstOrDefault(), json, waitForIndexing, cancellationToken);

            return response;
        }

        public async Task<List<ElasticBulkResponse>> PutCreatedMaterialsToElasticSearchAsync(IReadOnlyCollection<Guid> materialIds,
            bool waitForIndexing = false,
            CancellationToken cancellationToken = default)
        {
            var materials = await RunWithoutCommitAsync(_ => _.MaterialRepository.GetByIdsAsync(materialIds.ToHashSet(), IncludeAll));
            var materialDocuments = materials
                .Select(p => MapEntityToDocument(p))
                .ToDictionary(_ => _.Id);
            string json = materialDocuments.ConvertToJson();

            return await _elasticManager.PutDocumentsAsync(_elasticState.MaterialIndexes.FirstOrDefault(), json, waitForIndexing, cancellationToken);
        }

        public async Task<bool> PutMaterialToElasticSearchAsync(Guid materialId, CancellationToken ct = default, bool waitForIndexing = false)
        {
            var material = await RunWithoutCommitAsync(_ => _.MaterialRepository.GetByIdAsync(materialId, IncludeAll));

            return await PutMaterialToElasticSearchAsync(material, ct, waitForIndexing);
        }

        public async Task PutMaterialsToElasticSearchAsync(IEnumerable<Guid> materialIds, CancellationToken ct = default, bool waitForIndexing = false)
        {
            var materials = await RunWithoutCommitAsync(_ => _.MaterialRepository.GetByIdsAsync(materialIds.ToHashSet(), IncludeAll));

            foreach (var material in materials)
            {
                await PutMaterialToElasticSearchAsync(material, ct, waitForIndexing);
            }
        }

        public async Task<bool> PutMaterialToElasticSearchAsync(MaterialEntity material, CancellationToken ct = default, bool waitForIndexing = false)
        {
            var materialDocument = MapEntityToDocument(material);

            var materialIdList = material.Children
                                .Select(e => e.Id)
                                .Union(new[] { material.Id })
                                .ToArray();

            var responseList = await _mLResponseRepository.GetAllForMaterialListAsync(materialIdList);

            var responseDictionary = responseList
                                        .GroupBy(e => e.MaterialId)
                                        .ToDictionary(group => group.Key, group => group.ToArray());

            var (mlResponses, mlResponsesCount) = GetResponseJsonWithCounter(materialDocument.Id, responseDictionary);

            materialDocument.MLResponses = mlResponses;

            materialDocument.ProcessedMlHandlersCount = mlResponsesCount;

            materialDocument.ImageVectors = GetImageVectorList(materialIdList, responseDictionary);

            return await _elasticManager.PutDocumentAsync(_elasticState.MaterialIndexes.FirstOrDefault(),
                material.Id.ToString("N"),
                JsonConvert.SerializeObject(materialDocument),
                waitForIndexing,
                ct);
        }

        public async Task PutMaterialsToElasticByNodeIdsAsync(IReadOnlyCollection<Guid> nodeIds, CancellationToken ct = default, bool waitForIndexing = false)
        {
            var materials = await RunWithoutCommitAsync(_ => _.MaterialRepository.GetMaterialCollectionByNodeIdAsync(nodeIds, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures, MaterialIncludeEnum.WithFiles));
            await Task.WhenAll(materials.Select(_ => PutMaterialToElasticSearchAsync(_, ct, waitForIndexing)));
        }

        private static ImageVector[] GetImageVectorList(IReadOnlyCollection<Guid> materialIdList, Dictionary<Guid, MLResponseEntity[]> responseDictionary)
        {
            var result = new List<ImageVector>();

            foreach (var materialId in materialIdList)
            {
                if (responseDictionary.TryGetValue(materialId, out MLResponseEntity[] responseList))
                {
                    var imageVectorList = GetLatestImageVectorList(responseList, MlHandlerCodeList.ImageVector)
                                        .Select(e => new ImageVector(e))
                                        .ToArray();
                    result.AddRange(imageVectorList);
                }
            }
            return result.ToArray();
        }

        private async Task<(JObject mlResponses, int mlResponsesCount, ImageVector[] imageVector)> GetMLResponseData(Guid materialId)
        {
            var mlResponses = await _mLResponseRepository.GetAllForMaterialAsync(materialId);

            var imageVectorList = GetLatestImageVectorList(mlResponses, MlHandlerCodeList.ImageVector)
                                    .Select(e => new ImageVector(e))
                                    .ToArray();

            return (ConvertMLResponsesToJson(mlResponses), mlResponses.Count, imageVectorList);
        }

        private static IReadOnlyCollection<decimal[]> GetLatestImageVectorList(IReadOnlyCollection<MLResponseEntity> mlResponsesByEntity, string handlerCode)
        {
            var response = mlResponsesByEntity
                                    .OrderByDescending(e => e.ProcessingDate)
                                    .FirstOrDefault(e => e.HandlerCode == handlerCode);

            return FaceAPIResponseParser.GetFaceVectorList(response?.OriginalResponse);
        }

        private static (JObject responseJObject, int responsesCount) GetResponseJsonWithCounter(Guid materialId, Dictionary<Guid, MLResponseEntity[]> responseDictionary)
        {
            if (responseDictionary.TryGetValue(materialId, out MLResponseEntity[] responseList))
            {
                return (ConvertMLResponsesToJson(responseList), responseList.Length);
            }

            return (new JObject(), 0);
        }

        private static JObject ConvertMLResponsesToJson(IReadOnlyCollection<MLResponseEntity> mlResponses)
        {
            var mlResponsesContainer = new JObject();
            if (mlResponses.Any())
            {
                var mlHandlers = mlResponses.GroupBy(_ => _.HandlerName).ToArray();
                foreach (var mlHandler in mlHandlers)
                {
                    string propertyName = GetMlHandlerName(mlHandler);
                    mlResponsesContainer.Add(new JProperty(propertyName,
                            mlHandler.Select(p => p.OriginalResponse).ToArray()));
                }
            }
            return mlResponsesContainer;
        }

        private static string RemoveImagesFromContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;

            return Regex.Replace(content, @"\(data:image.+\)", string.Empty, RegexOptions.Compiled);
        }

        private static (string SortColumn, string SortOrder) MapSortingToElastic(SortingParams sorting)
        {
            return sorting.ColumnName switch
            {
                "createdDate" => ("CreatedDate", sorting.Order),
                "type" => ("Type.keyword", sorting.Order),
                "source" => ("Source.keyword", sorting.Order),
                "processedStatus" => ("ProcessedStatus.OrderNumber", sorting.Order),
                "sessionPriority" => ("SessionPriority.OrderNumber", sorting.Order),
                "importance" => ("Importance.OrderNumber", sorting.Order),
                "nodes" => ("ObjectsOfStudyCount", sorting.Order),
                "registrationDate" => ("RegistrationDate", sorting.Order),
                _ => (null, null)
            };
        }

        private static string GetMlHandlerName(IGrouping<string, MLResponseEntity> mlHandler)
        {
            var code = mlHandler.FirstOrDefault()?.HandlerCode;
            var propertyName = string.IsNullOrEmpty(code)
                ? mlHandler.Key.ToLowerCamelCase().RemoveWhiteSpace()
                : code;
            return propertyName;
        }

        private MaterialDocument MapEntityToDocument(MaterialEntity material)
        {
            var materialDocument = _mapper.Map<MaterialDocument>(material);

            materialDocument.Content = RemoveImagesFromContent(materialDocument.Content);

            materialDocument.Children = material.Children.Select(p => _mapper.Map<MaterialDocument>(p)).ToArray();

            var featureCollection = material.MaterialInfos
                .SelectMany(p => p.MaterialFeatures)
                .ToArray();
            materialDocument.NodeIds = featureCollection
                .Where(e => e.NodeLinkType == MaterialNodeLinkType.None)
                .Select(p => p.NodeId)
                .ToArray();

            materialDocument.NodesCount = materialDocument.NodeIds.Count();

            var nodeDictionary = MaterialDocumentHelper.MapFeatureCollectionToNodeDictionary(featureCollection, _ontologyData);

            var nodeFromSingsDictionary = MaterialDocumentHelper.GetObjectsLinkedBySign(nodeDictionary, _ontologyData);

            nodeDictionary.TryAddRange(nodeFromSingsDictionary);

            materialDocument.RelatedObjectCollection = MaterialDocumentHelper.MapObjectOfStudyCollection(nodeDictionary);

            materialDocument.RelatedEventCollection = MaterialDocumentHelper.MapEventCollection(nodeDictionary);

            materialDocument.RelatedSignCollection = MaterialDocumentHelper.MapSingCollection(nodeDictionary);

            materialDocument.ObjectsOfStudyCount = materialDocument.RelatedObjectCollection.Count(e => e.RelationType == NoneLinkTypeValue);

            return materialDocument;
        }

        private void ChangeAssigneeAggregations(Dictionary<string, AggregationItem> aggregations, Guid userId)
        {
            var item = aggregations.GetValueOrDefault(MaterialAliases.Assignees.Alias);
            if (item == null) return;

            item.Buckets = item.Buckets.Where(_ => _.Key == userId.ToString()).ToArray();
            if (item.Buckets.Length == 0) return;

            item.Buckets[0].Key = MaterialAliases.Assignees.AliasForSingleItem;
        }

        private IReadOnlyCollection<Property> ChangeAssigneeFiltered(IReadOnlyCollection<Property> items, Guid userId)
        {
            var result = new List<Property>(items.Select(_ => new Property(_.Name, _.Value)));
            var assigneeProperty = result.FirstOrDefault(_ => _.Name == MaterialAliases.Assignees.Alias);
            if (assigneeProperty?.Value == MaterialAliases.Assignees.AliasForSingleItem)
            {
                assigneeProperty.Value = userId.ToString();
            }

            return result;
        }

        private JObject ChangeAssigneeHighlight(JObject highlight, Guid userId)
        {
            if (highlight.ContainsKey(MaterialAliases.Assignees.Path))
            {
                highlight.Remove(MaterialAliases.Assignees.Path);
            }
            if (highlight.ContainsKey(MaterialAliases.Assignees.Alias))
            {
                var baseValue = highlight.GetValue(MaterialAliases.Assignees.Alias).ToString();
                if (baseValue.Contains(userId.ToString()))
                {
                    highlight[MaterialAliases.Assignees.Alias] = baseValue.Replace(userId.ToString(), MaterialAliases.Assignees.AliasForSingleItem);
                }
            }
            return highlight;
        }
    }
}