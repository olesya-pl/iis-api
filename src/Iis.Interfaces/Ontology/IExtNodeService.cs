using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Interfaces.Ontology
{
    public interface IExtNodeService
    {
        Task<List<Guid>> GetExtNodesByTypeIdsAsync(IEnumerable<string> typeNames, CancellationToken cancellationToken = default);

        Task<IExtNode> GetExtNodeAsync(Guid id, CancellationToken ct = default);
    }
}
