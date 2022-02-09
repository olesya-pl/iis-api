using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.SecurityLevels;
using Iis.MaterialDistributor.Contracts.Repositories;

namespace Iis.MaterialDistributor.Repositories
{
    public class SecurityLevelElasticRepository : ISecurityLevelElasticRepository
    {
        private readonly IElasticManager _elasticManager;

        public SecurityLevelElasticRepository(IElasticManager elasticManager)
        {
            _elasticManager = elasticManager;
        }

        public async Task<IReadOnlyList<SecurityLevelPlain>> GetSecurityLevelsPlainAsync(CancellationToken cancellationToken)
        {
            var response = await _elasticManager
                .WithDefaultUser()
                .GetSecurityLevelsAsync(cancellationToken);

            return response.Items.Select(_ => _.SearchResult.ToObject<SecurityLevelPlain>()).ToList();
        }
    }
}
