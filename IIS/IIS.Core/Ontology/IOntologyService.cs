﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Ontology
{
    public interface IOntologyService
    {
        Task<IEnumerable<Node>> GetNodesAsync(IEnumerable<Type> types, NodeFilter filter, CancellationToken cancellationToken = default);

        Task SaveNodeAsync(Node node, CancellationToken cancellationToken = default);

        Task RemoveNodeAsync(Node node, CancellationToken cancellationToken = default);
        Task<Node> LoadNodesAsync(Guid nodeId, IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default);
        Task<IEnumerable<Node>> LoadNodesAsync(IEnumerable<Guid> nodeIds, IEnumerable<EmbeddingRelationType> relationTypes, CancellationToken cancellationToken = default);
    }
}
