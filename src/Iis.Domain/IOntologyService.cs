﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Iis.Domain
{
    public interface IOntologyService
    {
        Task<int> GetNodesCountAsync(IEnumerable<NodeType> types, ElasticFilter filter, CancellationToken cancellationToken = default);
        Task<IEnumerable<Node>> GetNodesAsync(IEnumerable<NodeType> types, ElasticFilter filter, CancellationToken cancellationToken = default);
        Task<(IEnumerable<JObject> nodes, int count)> FilterObjectsOfStudyAsync(ElasticFilter filter, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Node> nodes, int count)> GetNodesAsync(IEnumerable<Guid> matchList, CancellationToken cancellationToken = default);
        Task SaveNodeAsync(Node node, CancellationToken cancellationToken = default);
        Task SaveNodesAsync(IEnumerable<Node> node, CancellationToken cancellationToken = default);
        Task RemoveNodeAsync(Node node, CancellationToken cancellationToken = default);
        Task<Node> LoadNodesAsync(Guid nodeId, IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default);
        Task<IEnumerable<Node>> LoadNodesAsync(IEnumerable<Guid> nodeIds, IEnumerable<EmbeddingRelationType> relationTypes, CancellationToken cancellationToken = default);


    }
}
