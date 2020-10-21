using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Iis.Interfaces.Ontology.Data;

namespace Iis.Interfaces.Ontology
{
    public interface IExtNodeService
    {
        List<IExtNode> GetExtNodes(IReadOnlyCollection<INode> itemsToUpdate);
        Task<IExtNode> GetExtNodeWithoutNestedObjectsAsync(Guid id, CancellationToken ct = default);
        IExtNode GetExtNode(INode nodeEntity);
    }
}
