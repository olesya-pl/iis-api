using Iis.DataModel.ChangeHistory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.DbLayer.Repositories
{
    public interface IChangeHistoryRepository
    {
        void Add(ChangeHistoryEntity entity);
        Task<List<ChangeHistoryEntity>> GetByIdsAsync(IEnumerable<Guid> ids);
        Task<List<ChangeHistoryEntity>> GetByRequestIdAsync(Guid requestId);
        Task<List<ChangeHistoryEntity>> GetManyAsync(Guid targetId, string propertyName, DateTime? dateFrom = null, DateTime? dateTo = null);
        Task<List<ChangeHistoryEntity>> GetManyAsync(IReadOnlyCollection<Guid> targetIdList, string propertyName, DateTime? dateFrom = null, DateTime? dateTo = null);
        void AddRange(IReadOnlyCollection<ChangeHistoryEntity> changes);
    }
}