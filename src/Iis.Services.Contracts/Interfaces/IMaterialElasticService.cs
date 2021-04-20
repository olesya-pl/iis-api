using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Params;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IMaterialElasticService
    {
        Task<SearchResult> SearchMaterialsByConfiguredFieldsAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default);
        Task<SearchResult> BeginSearchByScrollAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default);
        Task<SearchResult> SearchMaterialsAsync(Guid userId, SearchParams searchParams, IEnumerable<Guid> materialList, CancellationToken ct = default);
        Task<SearchResult> SearchMoreLikeThisAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default);
        Task<SearchResult> SearchByImageVector(Guid userId, IReadOnlyCollection<decimal[]> imageVectorList, PaginationParams page, CancellationToken ct = default);
        Task<int> CountMaterialsByConfiguredFieldsAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default);
        Task<SearchResult> SearchByScroll(Guid userId, string scrollId);
    }
}