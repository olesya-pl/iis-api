using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.SecurityLevels;

namespace Iis.MaterialDistributor.Contracts.Repositories
{
    public interface ISecurityLevelElasticRepository
    {
        Task<IReadOnlyList<SecurityLevelPlain>> GetSecurityLevelsPlainAsync(CancellationToken cancellationToken);
    }
}