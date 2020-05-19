using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Interfaces.Ontology
{
    public interface IChangeHistoryService
    {
        Task SaveChange(string attributeDotName, Guid targetId, string userName, string oldValue, string newValue);
        Task SaveChange(Guid typeId, Guid rootTypeId, Guid targetId, string userName, string oldValue, string newValue);
        Task<IReadOnlyList<IChangeHistoryItem>> GetChangeHistory(Guid targetId, string propertyName);
    }
}