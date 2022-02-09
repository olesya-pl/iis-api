using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.MaterialDistributor.DataStorage;

namespace Iis.MaterialDistributor.Contracts.Services
{
    public interface IMaterialDistributionService
    {
        Task<List<MaterialDistributionInfo>> GetMaterialCollectionAsync(int offsetHours, CancellationToken cancellationToken);
    }
}