using System;
using System.Collections.Generic;
using Iis.Interfaces.SecurityLevels;

namespace Iis.MaterialDistributor.Contracts.Services
{
    public class UserDistributionInfo
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public IReadOnlyList<UserChannelInfo> Channels { get; set; }
        public IReadOnlyList<ISecurityLevel> SecurityLevels { get; set; }
    }
}