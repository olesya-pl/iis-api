using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Domain.Graph;
namespace Iis.Services.Contracts.Interfaces
{
    public interface IGraphService
    {
        Task<GraphData> GetGraphDataForNodeListAsync(IReadOnlyCollection<Guid> nodeIdList);
    }
}