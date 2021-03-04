using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        private const string UsersIndexName = "_xpack/security/user";

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
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            return _elasticManager.PutExactPayloadAsync($"{UsersIndexName}/{elasticUser.Metadata.Id.ToString("N")}", serialized, cancellationToken);
        }
    }
}
