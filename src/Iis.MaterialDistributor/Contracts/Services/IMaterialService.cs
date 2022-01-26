using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Iis.MaterialDistributor.Contracts.Services
{
    public interface IMaterialService
    {
        Task<IReadOnlyCollection<MaterialDocument>> GetMaterialCollectionAsync(int offsetHours, CancellationToken cancellationToken);
    }
}