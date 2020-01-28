using Iis.DataModel;
using Iis.Domain.ExtendedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.Ontology
{
    public interface IExtNodeService
    {
        Task<ExtNode> GetExtNodeById(Guid id);
        Task<List<Guid>> GetNodeTypesForElastic();
        Task<List<ExtNode>> GetExtNodesByTypeIds(IEnumerable<Guid> nodeTypeIds);
    }
}
