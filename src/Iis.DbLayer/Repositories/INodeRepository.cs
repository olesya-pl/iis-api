﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;

using Newtonsoft.Json.Linq;

namespace Iis.DbLayer.Repositories
{
    public interface INodeRepository
    {
        Task<JObject> GetJsonNodeByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PutNodeAsync(Guid id, IEnumerable<string> fieldsToExclude, CancellationToken cancellationToken = default);
        Task<List<ElasticBulkResponse>> PutNodesAsync(IReadOnlyCollection<INode> nodes, IEnumerable<string> fieldsToExclude, CancellationToken ct);
        Task<List<ElasticBulkResponse>> PutNodesAsync(IReadOnlyCollection<INode> nodes, CancellationToken ct);
        Task<List<ElasticBulkResponse>> PutHistoricalNodesAsync(IReadOnlyCollection<INode> items, CancellationToken ct = default);
        Task<List<ElasticBulkResponse>> PutHistoricalNodesAsync(Guid id, Guid? requestId = null, CancellationToken ct = default);
    }
}
