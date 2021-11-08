using Iis.DataModel.ChangeHistory;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
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
        Task<IReadOnlyCollection<ChangeHistoryEntity>> GetAllAsync(int limit, int offset, Expression<Func<ChangeHistoryEntity, bool>> predicate = null, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(Expression<Func<ChangeHistoryEntity, bool>> predicate = null, CancellationToken cancellationToken = default);
        void AddRange(IReadOnlyCollection<ChangeHistoryEntity> changes);
        Task<ChangeHistoryEntity> GetLatestByIdAndPropertyWithNewValueAsync(Guid targetId, string propertyName, string expectedNewValue, CancellationToken ct = default);
        Task<ChangeHistoryEntity[]> GetManyLatestByIdAndPropertyWithNewValueAsync(IReadOnlyCollection<Guid> targetIdCollection, string propertyName, string expectedNewValue, CancellationToken ct = default);
    }
}