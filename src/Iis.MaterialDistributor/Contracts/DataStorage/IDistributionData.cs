using System;
using System.Collections.Generic;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.DataStorage;

namespace Iis.MaterialDistributor.Contracts.DataStorage
{
    public interface IDistributionData
    {
        void RefreshMaterials(Dictionary<Guid, MaterialDistributionInfo> materials);
        void Distribute(IReadOnlyList<UserDistributionInfo> users);
        MaterialDistributionInfo GetMaterialFromQueue(UserDistributionInfo user);
    }
}
