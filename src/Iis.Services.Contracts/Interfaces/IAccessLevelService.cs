using Iis.Interfaces.AccessLevels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IAccessLevelService
    {
        Task ChangeAccessLevels(IAccessLevels newAccessLevels, Dictionary<Guid, Guid> mappings, CancellationToken ct);
    }
}