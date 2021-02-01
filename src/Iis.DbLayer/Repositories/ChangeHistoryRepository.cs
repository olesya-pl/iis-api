using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.ChangeHistory;
using IIS.Repository;
using Microsoft.EntityFrameworkCore;

namespace Iis.DbLayer.Repositories
{
    public class ChangeHistoryRepository : RepositoryBase<OntologyContext>, IChangeHistoryRepository
    {
        public Task<List<ChangeHistoryEntity>> GetManyAsync(Guid targetId, string propertyName, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var query = Context.ChangeHistory.AsNoTracking().Where(ch => ch.TargetId == targetId);

            if (!string.IsNullOrEmpty(propertyName))
            {
                query = query.Where(ch => ch.PropertyName == propertyName);
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(e => e.Date >= dateFrom);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(e => e.Date <= dateTo);
            }

            return query.OrderByDescending(ch => ch.Date).ToListAsync();
        }

        public Task<List<ChangeHistoryEntity>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            return Context.ChangeHistory
                .AsNoTracking()
                .Where(x => ids.Contains(x.TargetId))
                .ToListAsync();
        }

        public Task<List<ChangeHistoryEntity>> GetByRequestIdAsync(Guid requestId)
        {
            return Context.ChangeHistory
                .AsNoTracking()
                .Where(ch => ch.RequestId == requestId)
                .ToListAsync();
        }

        public void Add(ChangeHistoryEntity entity)
        {
            Context.Add(entity);
        }

        public void AddRange(IReadOnlyCollection<ChangeHistoryEntity> entities)
        {
            Context.AddRange(entities);
        }
    }
}
