using System;
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

                if (!IsRegisteredContourUser(user)) continue;

                result.Add(user);
            }
            return result;
        }

        private static UserDistributionInfo GetUserDistributionInfo(JToken token)
        {
            var userName = ((JProperty)token).Name;

            return new UserDistributionInfo
            {
                Id = Guid.TryParse(userName, out Guid id) ? id : Guid.Empty,
                Username = userName
            };
        }

        private static bool IsRegisteredContourUser(UserDistributionInfo user)
        {
            return user.Id != Guid.Empty;
        }
    }
}
