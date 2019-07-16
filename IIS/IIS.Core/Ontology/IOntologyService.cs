using System;
using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Ontology
{
    public interface IOntologyService
    {
        Task SaveTypeAsync(Type type, CancellationToken cancellationToken = default);

        Task RemoveTypeAsync(string typeName, CancellationToken cancellationToken = default);

        Task SaveNodeAsync(Node node, CancellationToken cancellationToken = default);

        Task RemoveNodeAsync(Guid nodeId, CancellationToken cancellationToken = default);
    }
}
