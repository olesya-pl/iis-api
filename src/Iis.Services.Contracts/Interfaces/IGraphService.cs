using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Domain;
using Iis.Domain.Graph;
using Iis.Domain.Users;
using System.Threading;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IGraphService
    {
        Task<GraphData> GetGraphDataForNodeListAsync(IReadOnlyCollection<Guid> nodeIdList, User user, CancellationToken cancellationToken);
    }
}