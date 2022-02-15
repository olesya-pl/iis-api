using System;
using System.Collections.Generic;
using Iis.Interfaces.SecurityLevels;

namespace Iis.MaterialDistributor.DataStorage
{
    public class UserDistributionInfo
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public IReadOnlyList<UserChannelInfo> Channels { get; set; }
        public IReadOnlyList<ISecurityLevel> SecurityLevels { get; set; }
    }
}
