using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

using Iis.Domain.Elastic;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Elastic;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Services.Contracts.Enums;
using Iis.Services.Contracts.Params;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Interfaces.Elastic;

namespace Iis.Services
{
    public class MaterialElasticService : IMaterialElasticService
    {
        private readonly IElasticManager _elasticManager;
        private readonly IElasticState _elasticState;
        private readonly IMaterialRepository _materialRepository;
        private readonly IElasticResponseManagerFactory _elasticResponseManagerFactory;

        private string[] MaterialIndexes = new[] { "Materials" };

        public MaterialElasticService(IElasticManager elasticManager,
            IElasticState elasticState,
            IElasticResponseManagerFactory elasticResponseManagerFactory,
            IMaterialRepository materialRepository
            )
        {
            _elasticManager = elasticManager;
            _elasticState = elasticState;
            _materialRepository = materialRepository;
            _elasticResponseManagerFactory = elasticResponseManagerFactory;
        }

        public async Task<SearchResult> SearchMaterialsByConfiguredFieldsAsync(IElasticNodeFilter filter, CancellationToken ct = default)
        {
            var searchResponse = await _materialRepository.SearchMaterials(filter, ct);

            foreach (var item in searchResponse.Items)
            {
                item.Value.Highlight = await _elasticResponseManagerFactory.Create(SearchType.Material)
                 .GenerateHighlightsWithoutDublications(item.Value.SearchResult, item.Value.Highlight);
            }

            return searchResponse;
        }

        public async Task<SearchResult> SearchMaterialsAsync(SearchParams searchParams, IEnumerable<Guid> materialList, CancellationToken ct = default)
        {
            var queryBuilder = new BoolQueryBuilder()
                                .WithMust()
                                .WithPagination(searchParams.Offset, searchParams.Limit)
                                .WithDocumentList(materialList);

            if(!SearchQueryExtension.IsMatchAll(searchParams.Suggestion))
            {
                queryBuilder.WithExactQuery(searchParams.Suggestion);
            }

            var query = queryBuilder
                            .Build()
                            .WithHighlights()
                            .ToString(Formatting.None);

            var searchResult = await _elasticManager.SearchAsync(query, MaterialIndexes, ct);

            return new SearchResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult })
            };
        }

        public async Task<SearchResult> SearchMoreLikeThisAsync(SearchParams searchParams, CancellationToken ct = default)
        {
            var queryData = new MoreLikeThisQueryBuilder()
                        .WithPagination(searchParams.Offset, searchParams.Limit)
                        .WithMaterialId(searchParams.Suggestion)
                        .Build()
                        .ToString(Formatting.None);

            var searchResult = await _elasticManager.SearchAsync(queryData, _elasticState.MaterialIndexes, ct);

            return new SearchResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult })
            };
        }

        public async Task<SearchResult> SearchByImageVector(decimal[] imageVector, int offset, int size, CancellationToken ct = default)
        {
            var searchResult = await _elasticManager.SearchByImageVector(imageVector, new IisElasticSearchParams
            {
                BaseIndexNames = _elasticState.MaterialIndexes,
                From = offset,
                Size = size
            }, ct);
            return new SearchResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult })
            };
        }

        public Task<int> CountMaterialsByConfiguredFieldsAsync(IElasticNodeFilter filter, CancellationToken ct = default)
        {
            return _materialRepository.CountMaterialsAsync(filter, ct);
        }
    }
}