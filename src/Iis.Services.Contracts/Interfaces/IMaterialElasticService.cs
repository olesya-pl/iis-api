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
        Task<SearchResult> SearchMaterialsAsync(SearchParams searchParams, IEnumerable<Guid> materialList, CancellationToken ct = default);
    }
}