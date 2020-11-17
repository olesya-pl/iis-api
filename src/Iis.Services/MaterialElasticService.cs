using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Interfaces.Elastic;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using Newtonsoft.Json;

namespace Iis.Services
{
    public class MaterialElasticService : IMaterialElasticService
    {
        private readonly IElasticManager _elasticManager;

        private string[] MaterialIndexes = new[] { "Materials" };

        public MaterialElasticService(IElasticManager elasticManager)
        {
            _elasticManager = elasticManager;
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
    }
}