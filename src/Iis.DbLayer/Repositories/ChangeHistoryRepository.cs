﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
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

            query = AddDatePeriod(query, dateFrom, dateTo);

            return query.OrderByDescending(ch => ch.Date).ToListAsync();
        }

        public Task<List<ChangeHistoryEntity>> GetManyAsync(IReadOnlyCollection<Guid> targetIdList, string propertyName, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var query = Context.ChangeHistory.AsNoTracking().Where(ch => targetIdList.Contains(ch.TargetId));

            if (!string.IsNullOrEmpty(propertyName))
            {
                query = query.Where(ch => ch.PropertyName == propertyName);
            }

            query = AddDatePeriod(query, dateFrom, dateTo);

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

        public async Task<IReadOnlyCollection<ChangeHistoryEntity>> GetAllAsync(
            int limit,
            int offset,
            Expression<Func<ChangeHistoryEntity, bool>> predicate = null,
            CancellationToken cancellationToken = default)
        {
            var query = Context.ChangeHistory.AsNoTracking();
            if (predicate != null)
                query = query.Where(predicate);

            return await query.OrderBy(_ => _.Id)
                .Skip(offset)
                .Take(limit)
                .ToArrayAsync(cancellationToken);
        }

        public Task<int> GetTotalCountAsync(Expression<Func<ChangeHistoryEntity, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            var query = Context.ChangeHistory.AsNoTracking();
            if (predicate != null)
                query = query.Where(predicate);

            return query.CountAsync(cancellationToken);
        }

        public void Add(ChangeHistoryEntity entity)
        {
            Context.Add(entity);
        }

        public void AddRange(IReadOnlyCollection<ChangeHistoryEntity> entities)
        {
            Context.AddRange(entities);
        }

        private static IQueryable<ChangeHistoryEntity> AddDatePeriod(IQueryable<ChangeHistoryEntity> query, DateTime? dateFrom, DateTime? dateTo)
        {
            if (dateFrom.HasValue)
            {
                query = query.Where(e => e.Date >= dateFrom);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(e => e.Date <= dateTo);
            }
            return query;
        }
    }
}
