using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.Elastic;
using Iis.MaterialDistributor.Contracts.Repositories;
using Iis.MaterialDistributor.DataStorage;
using Newtonsoft.Json.Linq;

namespace Iis.MaterialDistributor.Repositories
{
    public class UserElasticRepository : IUserElasticRepository
    {
        private readonly IElasticManager _elasticManager;

        public UserElasticRepository(IElasticManager elasticManager)
        {
            _elasticManager = elasticManager;
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

        private UserDistributionInfo GetUserDistributionInfo(JToken token)
        {
            return new UserDistributionInfo
            {
                Username = ((JProperty)token).Name
            };
        }
    }
}
