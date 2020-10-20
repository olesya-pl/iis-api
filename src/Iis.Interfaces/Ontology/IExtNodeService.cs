using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Iis.Interfaces.Ontology.Data;

namespace Iis.Interfaces.Ontology
{
    public interface IExtNodeService
    {
        Task<List<Guid>> GetExtNodesByTypeIdsAsync(IEnumerable<string> typeNames, CancellationToken cancellationToken = default);

        Task<IExtNode> GetExtNodeAsync(Guid id, CancellationToken ct = default);
        List<IExtNode> GetExtNodes(IReadOnlyCollection<INode> itemsToUpdate);
        Task<IExtNode> GetExtNodeWithoutNestedObjectsAsync(Guid id, CancellationToken ct = default);
        IExtNode GetExtNode(INode nodeEntity);
    }
}
