using System.Threading;
using System.Threading.Tasks;

namespace Iis.MaterialDistributor.Contracts.DataStorage
{
    public interface IDistributionDataMediator
    {
        Task DistributeAsync(CancellationToken cancellationToken);
        Task RefreshMaterialsAsync(CancellationToken cancellationToken);
    }
}