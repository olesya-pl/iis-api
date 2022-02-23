using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Iis.Interfaces.Elastic;
using Iis.MaterialDistributor.Contracts.Repositories;

namespace Iis.MaterialDistributor.Repositories
{
    public class UserElasticRepository : IUserElasticRepository
    {
        private const string UserNamePropertyName = "username";
        private const string EnabledPropertyName = "enabled";
        private const string MetadataPropertyName = "metadata";
        private const string ChannelsPropertyName = "channels";

        private static readonly UserDistributionEntity EmptyUserEntity = new UserDistributionEntity { Id = Guid.Empty };
        private readonly IElasticManager _elasticManager;

        public UserElasticRepository(IElasticManager elasticManager)
        {
            _elasticManager = elasticManager;
        }

        public async Task<IReadOnlyList<UserDistributionEntity>> GetOperatorsAsync(CancellationToken cancellationToken)
        {
            var jObject = await _elasticManager
                .WithDefaultUser()
                .GetUsersAsync(cancellationToken);

            var result = new List<UserDistributionEntity>();

            foreach (var token in jObject.Children())
            {
                var user = GetUserDistributionInfo(token);

                if (!IsRegisteredContourUser(user)) continue;

                if (!user.Enabled) continue;

                result.Add(user);
            }
            return result;
        }

        private static UserDistributionEntity GetUserDistributionInfo(JToken token)
        {
            var tokenName = ((JProperty)token).Name;

            if (!Guid.TryParse(tokenName, out Guid userId))
            {
                return EmptyUserEntity;
            }

            var tokenValue = token
                            .Children<JObject>()
                            .FirstOrDefault();

            if (tokenValue is null)
            {
                return EmptyUserEntity;
            }

            var metadataTokenValue = tokenValue[MetadataPropertyName];

            return new UserDistributionEntity
            {
                Id = userId,
                UserName = tokenValue.Value<string>(UserNamePropertyName),
                Enabled = tokenValue.Value<bool>(EnabledPropertyName),
                Channels = GetUserChannelEntities(metadataTokenValue)
            };
        }

        private static bool IsRegisteredContourUser(UserDistributionEntity user)
        {
            return user.Id != Guid.Empty;
        }

        private static IReadOnlyList<UserChannelEntity> GetUserChannelEntities(JToken metadataToken)
        {
            var channelArray = metadataToken.Value<JArray>(ChannelsPropertyName);

            if (channelArray is null) return Array.Empty<UserChannelEntity>();

            return JsonConvert.DeserializeObject<IReadOnlyList<UserChannelEntity>>(channelArray.ToString());
        }
    }
}
