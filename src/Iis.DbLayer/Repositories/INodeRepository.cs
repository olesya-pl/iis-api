using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Iis.DbLayer.Repositories
{
    public interface INodeRepository
    {
        Task<JObject> GetJsonNodeByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PutHistoricalNodesAsync(Guid id, Guid? requestId = null, CancellationToken ct = default);
    }
}
