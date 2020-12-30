using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Params;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IChangeHistoryService
    {
        Task SaveMaterialChanges(IReadOnlyCollection<ChangeHistoryDto> changes);
        Task SaveNodeChange(string attributeDotName, Guid targetId, string userName, string oldValue, string newValue, string parentTypeName, Guid requestId);
        Task<List<ChangeHistoryDto>> GetChangeHistory(ChangeHistoryParams parameters);
        Task<List<ChangeHistoryDto>> GetChangeHistory(IEnumerable<Guid> ids);
        Task<List<ChangeHistoryDto>> GetChangeHistoryByRequest(Guid requestId);
        Task<List<ChangeHistoryDto>> GetLocationHistory(Guid entityId);
    }
}