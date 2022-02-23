using System;
using System.Linq;
using System.Collections.Generic;
using Iis.MaterialDistributor.Contracts.Services;

namespace Iis.MaterialDistributor.Services
{
    internal class UserChannelCoefficientData
    {
        private readonly IReadOnlyDictionary<string, UserChannelInfo> _channelDictionary;

        public UserChannelCoefficientData(UserDistributionInfo userInfo)
        {
            UserId = userInfo.Id;
            UserName = userInfo.UserName;

            _channelDictionary = (userInfo.Channels ?? Array.Empty<UserChannelInfo>())
                                    .ToDictionary(_ => _.Channel);
        }

        public Guid UserId { get; private set; }
        public string UserName { get; private set; }

        public UserChannelInfo Get(string channel)
        {
            return _channelDictionary.TryGetValue(channel, out UserChannelInfo channelInfo)
            ? channelInfo
            : UserChannelInfo.Default;
        }
    }
}