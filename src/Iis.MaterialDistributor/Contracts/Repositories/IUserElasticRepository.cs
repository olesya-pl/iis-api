using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.MaterialDistributor.Contracts.Repositories
{
    public interface IUserElasticRepository
    {
        Task<IReadOnlyList<UserDistributionEntity>> GetOperatorsAsync(CancellationToken cancellationToken);
    }
}