using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Interfaces.Ontology
{
    public interface IChangeHistoryService
    {
        Task SaveChange(string attributeDotName, Guid targetId, string userName, string oldValue, string newValue, Guid requestId);
        Task<IReadOnlyList<IChangeHistoryItem>> GetChangeHistory(Guid targetId, string propertyName, DateTime? dateFrom = null, DateTime? dateTo = null);
        Task<IReadOnlyList<IChangeHistoryItem>> GetChangeHistory(IEnumerable<Guid> ids);
        Task<IReadOnlyList<IChangeHistoryItem>> GetChangeHistoryByRequest(Guid requestId);
    }
}