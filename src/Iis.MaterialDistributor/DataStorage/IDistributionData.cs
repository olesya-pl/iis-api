using Iis.MaterialDistributor.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.MaterialDistributor.DataStorage
{
    public interface IDistributionData
    {
        void RefreshMaterialsAsync(Dictionary<Guid, MaterialDistributionInfo> materials);
        void Distribute(IReadOnlyList<UserDistributionInfo> users);
        MaterialDistributionInfo GetMaterialFromQueue(UserDistributionInfo user);
    }
}
