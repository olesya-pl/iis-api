using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Services.Contracts.Dtos.Roles;

namespace Iis.Services.Contracts.Interfaces.Roles
{
    public interface IGroupsService
    {
        Task<GroupDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<GroupAccessDto>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}