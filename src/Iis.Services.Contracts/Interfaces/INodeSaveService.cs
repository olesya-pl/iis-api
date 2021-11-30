using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Newtonsoft.Json.Linq;

namespace Iis.Services.Contracts.Interfaces
{
    public interface INodeSaveService
    {
        Task<JObject> GetJsonNodeByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PutNodeAsync(Guid id, IEnumerable<string> fieldsToExclude, CancellationToken cancellationToken = default);
        Task<List<ElasticBulkResponse>> PutNodesAsync(IReadOnlyCollection<INode> nodes, IEnumerable<string> fieldsToExclude, CancellationToken ct);
        Task<List<ElasticBulkResponse>> PutNodesAsync(IReadOnlyCollection<INode> nodes, CancellationToken ct);
    }
}