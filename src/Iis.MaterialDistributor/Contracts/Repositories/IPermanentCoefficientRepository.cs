using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.MaterialDistributor.Contracts.Repositories
{
    public interface IPermanentCoefficientRepository
    {
        Task<IReadOnlyDictionary<string, int>> GetAsync(CancellationToken cancellationToken);
    }
}