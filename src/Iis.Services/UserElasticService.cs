using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.Elastic;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Iis.Services
{
    internal class UserElasticService : IUserElasticService
    {
        private readonly IElasticManager _elasticManager;

        public UserElasticService(IElasticManager elasticManager)
        {
            _elasticManager = elasticManager;
        }

        public Task SaveAllUsersAsync(IReadOnlyCollection<ElasticUserDto> elasticUsers, CancellationToken cancellationToken)
        {
            var tasks = elasticUsers.Select(p => SaveUserAsync(p, cancellationToken));
            return Task.WhenAll(tasks);
        }

        public Task SaveUserAsync(ElasticUserDto elasticUser, CancellationToken cancellationToken)
        {
            var serialized = JsonConvert.SerializeObject(elasticUser, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            });
            return _elasticManager.PutExactPayloadAsync($"{ElasticConstants.UsersIndexName}/{elasticUser.Metadata.Id.ToString("N")}", serialized, cancellationToken);
        }

        public async Task ClearNonPredefinedUsers(CancellationToken cancellationToken)
        {
            var users = 
                await _elasticManager.GetExactPayloadAsyncDictionaryAsync<Dictionary<string, ElasticUserDto>>($"{ElasticConstants.UsersIndexName}", cancellationToken);
            var nonPredefinedUsers = users.Where(p => !p.Value.Metadata.Reserved.HasValue || p.Value.Metadata.Reserved == false).Select(p => p.Key);
            var tasks = new List<Task>();

            foreach (var username in nonPredefinedUsers)
            {
                tasks.Add(_elasticManager.DeleteExactPayloadAsync($"{ElasticConstants.UsersIndexName}/{username}", cancellationToken));
            }
            if (tasks.Any())
            {
                await Task.WhenAll(tasks);
            }
        }
    }
}
