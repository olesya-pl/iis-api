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
    internal class DistributionElasticRepository : IDistributionElasticRepository
    {
        private const int ZeroScrollDurationMinutes = 0;
        private static readonly string[] _materialIndexes = { "Materials" };
        private readonly IElasticManager _elasticManager;
        private readonly ElasticConfiguration _elasticConfiguration;

        public DistributionElasticRepository(
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

        public async Task<IReadOnlyList<UserDistributionInfo>> GetOperatorsAsync(CancellationToken cancellationToken)
        {
            var jObject = await _elasticManager
                .WithDefaultUser()
                .GetUsersAsync(cancellationToken);

            var result = new List<UserDistributionInfo>();
            foreach (var token in jObject.Children())
            {
                var user = GetUserDistributionInfo(token);
                result.Add(user);
            }
            return result;
        }

        public async Task<IReadOnlyList<SecurityLevelPlain>> GetSecurityLevelsPlainAsync(CancellationToken cancellationToken)
        {
            var response = await _elasticManager
                .WithDefaultUser()
                .GetSecurityLevelsAsync(cancellationToken);

            return response.Items.Select(_ => _.SearchResult.ToObject<SecurityLevelPlain>()).ToList();
        }

        private TimeSpan GetScrollDuration()
        {
            return TimeSpan.FromMinutes(
                _elasticConfiguration.ScrollDurationMinutes == ZeroScrollDurationMinutes
                ? ElasticConstants.DefaultScrollDurationMinutes
                : _elasticConfiguration.ScrollDurationMinutes);
        }

        private UserDistributionInfo GetUserDistributionInfo(JToken token)
        {
            return new UserDistributionInfo
            {
                Username = ((JProperty)token).Name
            };
        }
    }
}