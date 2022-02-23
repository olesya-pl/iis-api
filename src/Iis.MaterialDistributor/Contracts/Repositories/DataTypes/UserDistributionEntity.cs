using System;
using System.Collections.Generic;
using Iis.Interfaces.SecurityLevels;

namespace Iis.MaterialDistributor.Contracts.Repositories
{
    public class UserDistributionEntity
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public IReadOnlyList<UserChannelEntity> Channels { get; set; }
        public IReadOnlyList<ISecurityLevel> SecurityLevels { get; set; }
        public bool Enabled { get; set; }
    }
}