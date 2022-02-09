using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.MaterialDistributor.DataStorage;

namespace Iis.MaterialDistributor.Contracts.Repositories
{
    public interface IUserElasticRepository
    {
        Task<IReadOnlyList<UserDistributionInfo>> GetOperatorsAsync(CancellationToken cancellationToken);
    }
}