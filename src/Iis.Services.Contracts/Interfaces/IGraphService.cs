using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Domain;
using Iis.Domain.Graph;
using Iis.Domain.Users;
using Iis.Interfaces.SecurityLevels;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IGraphService
    {
        Task<GraphData> GetGraphDataForNodeListAsync(IReadOnlyCollection<Guid> nodeIdList, ISecurityLevelChecker securityLevelChecker, IOntologyService ontologyService, User user);
    }
}