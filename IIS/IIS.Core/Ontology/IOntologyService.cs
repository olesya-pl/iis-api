using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Ontology
{
    public interface IOntologyService
    {
        Task<IEnumerable<Node>> GetNodesByTypeAsync(Type type, CancellationToken cancellationToken = default);

        Task<IDictionary<string, IEnumerable<Node>>> GetNodesByTypesAsync(IEnumerable<string> typeNames,
            CancellationToken cancellationToken = default);

        Task<Node> LoadNodesAsync(Node source, IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default);

        Task<IDictionary<Guid, Node>> LoadNodesAsync(IEnumerable<Guid> sourceIds,
            IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default);

        Task SaveTypeAsync(Type type, CancellationToken cancellationToken = default);

        Task RemoveTypeAsync(string typeName, CancellationToken cancellationToken = default);

        Task SaveNodeAsync(Node node, CancellationToken cancellationToken = default);

        Task RemoveNodeAsync(Guid nodeId, CancellationToken cancellationToken = default);
    }
}
