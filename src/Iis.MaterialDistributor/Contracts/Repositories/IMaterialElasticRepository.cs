using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.SecurityLevels;
using Iis.MaterialDistributor.DataStorage;

namespace Iis.MaterialDistributor.Contracts.Repositories
{
    public interface IMaterialElasticRepository
    {
        Task<SearchResult> BeginSearchByScrollAsync(SearchParams searchParams, CancellationToken cancellationToken);
        Task<SearchResult> SearchByScrollAsync(string scrollId, CancellationToken cancellationToken);
    }
}