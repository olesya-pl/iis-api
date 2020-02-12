using Iis.DataModel;
using Iis.Domain.ExtendedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Ontology
{
    public interface IExtNodeService
    {
        Task<ExtNode> GetExtNodeByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<NodeTypeEntity>> GetNodeTypesForElasticAsync(CancellationToken cancellationToken = default);
        Task<List<ExtNode>> GetExtNodesByTypeIdsAsync(IEnumerable<Guid> nodeTypeIds, CancellationToken cancellationToken = default);
    }
}
