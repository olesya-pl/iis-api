using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Iis.MaterialDistributor.Contracts.Services
{
    public interface IMaterialDistributionService
    {
        Task<IReadOnlyCollection<MaterialDistributionInfo>> GetMaterialCollectionAsync(int offsetHours, CancellationToken cancellationToken);
    }
}