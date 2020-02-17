using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Interfaces.Ontology
{
    public interface IExtNodeService
    {
        Task<IExtNode> GetExtNodeByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<IExtNode>> GetExtNodesByTypeIdsAsync(List<string> typeNames, CancellationToken cancellationToken = default);
    }
}
