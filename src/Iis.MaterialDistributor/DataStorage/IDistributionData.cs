using Iis.MaterialDistributor.Contracts.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.MaterialDistributor.DataStorage
{
    public interface IDistributionData
    {
        Task RefreshMaterialsAsync(CancellationToken cancellationToken);
        Task Distribute(CancellationToken cancellationToken);
        MaterialDistributionInfo GetMaterialFromQueue(UserDistributionInfo user);
    }
}
