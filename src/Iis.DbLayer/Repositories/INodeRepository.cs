using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.Elastic;
using Newtonsoft.Json.Linq;

namespace Iis.DbLayer.Repositories
{
    public interface INodeRepository
    {
        Task<JObject> GetJsonNodeByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ElasticBulkResponse>> PutNodesAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
        Task<bool> PutHistoricalNodesAsync(Guid id, Guid? requestId = null, CancellationToken ct = default);

        Task<List<ElasticBulkResponse>> PutHistoricalNodesAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    }
}
