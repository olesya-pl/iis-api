using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Domain.Graph;
namespace Iis.Services.Contracts.Interfaces
{
    public interface IGraphService
    {
        Task<(IReadOnlyCollection<GraphLink> LinkList, IReadOnlyCollection<GraphNode> NodeList)> GetGraphDataForNodeListAsync(IReadOnlyCollection<Guid> nodeIdList, IReadOnlyCollection<Guid> relationTypeList);
    }
}