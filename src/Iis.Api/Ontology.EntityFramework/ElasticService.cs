﻿using Iis.DbLayer.Repositories;
using Iis.Domain.Elastic;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Ontology.EntityFramework
{
    public class ElasticService : IElasticService
    {
        private readonly IElasticManager _elasticManager;
        private readonly IElasticConfiguration _elasticConfiguration;
        private readonly INodeRepository _nodeRepository;
        private readonly IElasticState _elasticState;
        private const decimal HistoricalSearchBoost = 0.05m;

        public ElasticService(
            IElasticManager elasticManager,
            IElasticConfiguration elasticConfiguration,
            INodeRepository nodeRepository,
            IElasticState elasticState)
        {
            _elasticManager = elasticManager;
            _elasticConfiguration = elasticConfiguration;
            _nodeRepository = nodeRepository;
            _elasticState = elasticState;
        }

        public Task<int> CountByAllFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken ct = default)
        {
            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                Query = $"*{filter.Suggestion}*"
            };

            return _elasticManager.CountAsync(searchParams, ct);
        }

        public async Task<SearchResult> SearchByConfiguredFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken ct = default)
        {
            var ontologyFields
                = _elasticConfiguration.GetOntologyIncludedFields(typeNames.Where(p => _elasticState.OntologyIndexes.Contains(p)));

            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                Query = string.IsNullOrEmpty(filter.Suggestion) ? "*" : $"{filter.Suggestion}",
                From = filter.Offset,
                Size = filter.Limit,
                SearchFields = ontologyFields
            };
            var searchResult = await _elasticManager.SearchAsync(searchParams, ct);
            return new SearchResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult })
            };
        }

        public Task<int> CountByConfiguredFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken ct = default)
        {
            var ontologyFields
                = _elasticConfiguration.GetOntologyIncludedFields(typeNames.Where(p => _elasticState.OntologyIndexes.Contains(p)));

            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                Query = string.IsNullOrEmpty(filter.Suggestion) ? "*" : $"{filter.Suggestion}",
                From = filter.Offset,
                Size = filter.Limit,
                SearchFields = ontologyFields
            };
            return _elasticManager.CountAsync(searchParams, ct);
        }

        public async Task<SearchEntitiesByConfiguredFieldsResult> SearchEntitiesByConfiguredFieldsAsync(
            IEnumerable<string> typeNames, 
            IElasticNodeFilter filter, 
            CancellationToken ct = default)
        {
            if(SearchQueryExtension.IsMatchAll(filter.Suggestion))
            {
                var aggregadionFieldNameList = _elasticConfiguration
                    .GetOntologyIncludedFields(typeNames.Where(p => _elasticState.OntologyIndexes.Contains(p)))
                    .Where(f => f.IsAggregated)
                    .Select(f => f.Name)
                    .ToList()
                    .AsReadOnly();

                var query = new MatchAllQueryBuilder()
                            .WithPagination(filter.Offset, filter.Limit)
                            .Build()
                            .WithHighlights()
                            .WithAggregation(aggregadionFieldNameList)
                            .ToString(Formatting.None);

                var results = await _elasticManager.SearchAsync(query, typeNames, ct);

                return results.ToOutputSearchResult();
            }

            var (multiSearchParams, historicalResult) = await PrepareMultiElasticSearchParamsAsync(typeNames, filter, ct);

            var searchResult = await _elasticManager.SearchAsync(multiSearchParams, ct);

            if (historicalResult != null && historicalResult.Count > 0)
            {
                var highlightsById = historicalResult.Items
                    .GroupBy(x => x.SearchResult["Id"].Value<string>())
                    .ToDictionary(k => k.Key, v => v.First().Higlight);

                foreach (var item in searchResult.Items)
                {
                    item.SearchResult["highlight"] = CombineHighlights(
                        highlightsById.GetValueOrDefault(item.Identifier),
                        item.Higlight,
                        item.Identifier);
                }
            }

            return searchResult.ToOutputSearchResult();
        }

        public async Task<int> CountEntitiesByConfiguredFieldsAsync(
            IEnumerable<string> typeNames,
            IElasticNodeFilter filter,
            CancellationToken ct = default)
        {
            var (multiSearchParams, _)= await PrepareMultiElasticSearchParamsAsync(typeNames, filter, ct);
            return await _elasticManager.CountAsync(multiSearchParams, ct);
        }

        private async Task<(MultiElasticSearchParams MultiSearchParams, IElasticSearchResult HistoricalResult)> PrepareMultiElasticSearchParamsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken ct = default) 
        {
            var useHistoricalSearch = !string.IsNullOrEmpty(filter.Suggestion);

            var searchFields = _elasticConfiguration
                        .GetOntologyIncludedFields(typeNames.Where(p => _elasticState.OntologyIndexes.Contains(p))).ToList();

            IElasticSearchResult searchByHistoryResult = null;
            if (useHistoricalSearch)
            {
                var historicalIndexes = typeNames.Select(GetHistoricalIndex).ToList();

                if (SearchQueryExtension.IsExactQuery(filter.Suggestion))
                {
                    var exactQuery = new ExactQueryBuilder()
                        .WithResultFields(new List<string> { "Id" })
                        .WithPagination(0, filter.Limit)
                        .WithQueryString(filter.Suggestion)
                        .WithLeniency(true)
                        .Build();

                    searchByHistoryResult = await _elasticManager.SearchAsync(exactQuery.ToString(), historicalIndexes, ct);
                }
                else
                {
                    var searchByHistoryParams = new IisElasticSearchParams
                    {
                        BaseIndexNames = historicalIndexes,
                        Query = $"{filter.Suggestion}",
                        From = 0,
                        Size = filter.Limit,
                        SearchFields = searchFields,
                        ResultFields = new List<string> { "Id" }
                    };

                    searchByHistoryResult = await _elasticManager.SearchAsync(searchByHistoryParams, ct);
                }

            }

            var multiSearchParams = new MultiElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                From = filter.Offset,
                Size = filter.Limit,
                SearchParams = new List<(string Query, List<IIisElasticField> Fields)>
                {
                    (string.IsNullOrEmpty(filter.Suggestion) ? "*" : $"{filter.Suggestion}", searchFields)
                }
            };

            if (useHistoricalSearch && searchByHistoryResult.Count > 0)
            {
                var entityIds = searchByHistoryResult.Items.Select(x => x.SearchResult["Id"].Value<string>()).Distinct();
                multiSearchParams.SearchParams.Add((string.Join(" ", entityIds),
                    new List<IIisElasticField>
                    {
                        new IisElasticField
                        {
                            Name = "Id",
                            Boost = HistoricalSearchBoost
                        }
                    }));
            }

            return (multiSearchParams, searchByHistoryResult);
        }

        private JToken CombineHighlights(JToken historicalHighlights, JToken actualHighlights, string entityId)
        {
            if (historicalHighlights == null)
                return actualHighlights;

            var result = DeepCloneWithNewPrefix(historicalHighlights, "historical");
            if (actualHighlights == null)
                return result;

            foreach (var item in ((JObject)actualHighlights).Children<JProperty>())
            {
                if (item.Name == "Id" && item.Value[0].Value<string>().Contains(entityId))
                    continue;

                result.TryAdd(item.Name, item.Value);
            }

            return result;
        }

        private JObject DeepCloneWithNewPrefix(JToken token, string prefix)
        {
            var result = new JObject();
            foreach (var jProp in token.Children<JProperty>())
            {
                result.Add($"{prefix}.{jProp.Name}", jProp.Value);
            }

            return result;
        }

        private string GetHistoricalIndex(string typeName)
        {
            return $"historical_{typeName}";
        }

        public Task<bool> PutNodeAsync(Guid id, CancellationToken ct = default)
        {
            return _nodeRepository.PutNodeAsync(id, ct);
        }

        public Task<bool> PutNodeAsync(Guid id, IEnumerable<string> fieldsToExtract, CancellationToken ct = default)
        {
            return _nodeRepository.PutNodeAsync(id, fieldsToExtract, ct);
        }

        public Task<bool> PutHistoricalNodesAsync(Guid id, Guid? requestId = null, CancellationToken ct = default)
        {
            return _nodeRepository.PutHistoricalNodesAsync(id, requestId, ct);
        }

        public bool TypesAreSupported(IEnumerable<string> typeNames)
        {
            return OntologyIndexesAreSupported(typeNames);
        }

        private bool OntologyIndexIsSupported(string indexName)
        {
            return _elasticState.OntologyIndexes.Any(index => index.Equals(indexName)) || _elasticState.EventIndexes.Any(index => index.Equals(indexName));
        }

        private bool OntologyIndexesAreSupported(IEnumerable<string> indexNames)
        {
            return indexNames.All(indexName => OntologyIndexIsSupported(indexName));
        }

        public async Task<bool> PutNodesAsync(IReadOnlyCollection<INode> itemsToUpdate, CancellationToken ct)
        {
            var response = await _nodeRepository.PutNodesAsync(itemsToUpdate, ct);

            return response.All(x => x.IsSuccess);
        }

        public async Task<IEnumerable<IElasticSearchResultItem>> SearchByFieldsAsync(string query, string[] fieldNames, int size, CancellationToken ct = default)
        {
            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = _elasticState.OntologyIndexes.ToList(),
                Query = query,
                Size = size,
                SearchFields = fieldNames.Select(x => new IisElasticField { Name = x}).ToList()
            };
            var searchResult = await _elasticManager.SearchAsync(searchParams, ct);
            return searchResult.Items;
        }

        public async Task<SearchResult> SearchSignsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken ct = default)
        {
            var queryData = new ExactQueryBuilder()
                .WithPagination(filter.Offset, filter.Limit)
                .WithQueryString(filter.Suggestion)
                .Build()
                .SetupSorting("CreatedAt", "asc")
                .ToString(Formatting.None);

            var searchResult = await _elasticManager.SearchAsync(queryData, typeNames, ct);

            return new SearchResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult })
            };
        }
    }
}
