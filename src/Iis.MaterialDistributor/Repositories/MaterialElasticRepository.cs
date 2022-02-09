using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Iis.Elastic;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.SecurityLevels;
using Iis.MaterialDistributor.Contracts.Repositories;
using Iis.MaterialDistributor.DataStorage;
using Newtonsoft.Json.Linq;

namespace Iis.MaterialDistributor.Repositories
{
    internal class MaterialElasticRepository : IMaterialElasticRepository
    {
        private const int ZeroScrollDurationMinutes = 0;
        private static readonly string[] _materialIndexes = { "Materials" };
        private readonly IElasticManager _elasticManager;
        private readonly ElasticConfiguration _elasticConfiguration;

        public MaterialElasticRepository(
            IElasticManager elasticManager,
            ElasticConfiguration elasticConfiguration)
        {
            _elasticManager = elasticManager;
            _elasticConfiguration = elasticConfiguration;
        }

        public async Task<SearchResult> BeginSearchByScrollAsync(SearchParams searchParams, CancellationToken cancellationToken)
        {
            var query = new ExactQueryBuilder()
                .WithResultFields(searchParams.ResultFieldCollection)
                .WithPagination(searchParams.Pagination)
                .WithQueryString(searchParams.Suggestion)
                .BuildSearchQuery();

            var elasticResult = await _elasticManager
                .WithDefaultUser()
                .BeginSearchByScrollAsync(query.ToString(), GetScrollDuration(), _materialIndexes, cancellationToken);

            return elasticResult.ToSearchResult();
        }

        public async Task<SearchResult> SearchByScrollAsync(string scrollId, CancellationToken cancellationToken)
        {
            var elasticResult = await _elasticManager
                .WithDefaultUser()
                .SearchByScrollAsync(scrollId, GetScrollDuration(), cancellationToken);
            return elasticResult.ToSearchResult();
        }

        private TimeSpan GetScrollDuration()
        {
            return TimeSpan.FromMinutes(
                _elasticConfiguration.ScrollDurationMinutes == ZeroScrollDurationMinutes
                ? ElasticConstants.DefaultScrollDurationMinutes
                : _elasticConfiguration.ScrollDurationMinutes);
        }
    }
}