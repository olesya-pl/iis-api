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
        Task<SearchResult> SearchMaterialsByConfiguredFieldsAsync(SearchParams searchParams, CancellationToken ct = default);
        Task<SearchResult> SearchMaterialsAsync(SearchParams searchParams, IEnumerable<Guid> materialList, CancellationToken ct = default);
        Task<SearchResult> SearchMoreLikeThisAsync(SearchParams searchParams, CancellationToken ct = default);
        Task<SearchResult> SearchByImageVector(decimal[] imageVector, PaginationParams page, CancellationToken ct = default);
        Task<int> CountMaterialsByConfiguredFieldsAsync(SearchParams searchParams, CancellationToken ct = default);

    }
}