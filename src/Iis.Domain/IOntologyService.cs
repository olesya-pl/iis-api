using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Domain;

namespace Iis.Domain
{
    public interface IOntologyService
    {
        Task<int> GetNodesCountAsync(IEnumerable<NodeType> types, NodeFilter filter, CancellationToken cancellationToken = default);
        Task<IEnumerable<Node>> GetNodesAsync(IEnumerable<NodeType> types, NodeFilter filter, CancellationToken cancellationToken = default);
        Task SaveNodeAsync(Node node, CancellationToken cancellationToken = default);
        Task SaveNodesAsync(IEnumerable<Node> node, CancellationToken cancellationToken = default);

        Task RemoveNodeAsync(Node node, CancellationToken cancellationToken = default);
        Task<Node> LoadNodesAsync(Guid nodeId, IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default);
        Task<IEnumerable<Node>> LoadNodesAsync(IEnumerable<Guid> nodeIds, IEnumerable<EmbeddingRelationType> relationTypes, CancellationToken cancellationToken = default);
    }
}
