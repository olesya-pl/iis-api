using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.FlightRadar;
using IIS.Repository;
using Microsoft.EntityFrameworkCore;

namespace Iis.DbLayer.Repositories
{
    internal class FlightRadarRepository : RepositoryBase<OntologyContext>, IFlightRadarRepository
    {
        public async Task AddSyncJobConfigAsync(FlightRadarHistorySyncJobConfig configToAdd)
        {
             await Context.FlightRadarHistorySyncJobConfig.AddAsync(configToAdd);
        }

        public async Task<FlightRadarHistorySyncJobConfig> GetLastProcessedIdAsync()
        {
            return await Context.FlightRadarHistorySyncJobConfig.FirstOrDefaultAsync();
        }

        public void RemoveSyncJobConfig()
        {
            Context.FlightRadarHistorySyncJobConfig.RemoveRange(Context.FlightRadarHistorySyncJobConfig);
        }

        public Task SaveAsync(IReadOnlyCollection<LocationHistoryEntity> entities)
        {
            return Context.AddRangeAsync(entities);
        }

        public async Task<List<LocationHistoryEntity>> GetLocationHistoryAsync(Guid entityId)
        {
            return await Context.LocationHistory
                .Where(lh => lh.EntityId == entityId)
                .OrderByDescending(lh => lh.RegisteredAt)
                .ToListAsync();
        }

        public Task<List<LocationHistoryEntity>> GetLocationHistory(Guid entityId, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = Context.LocationHistory.Where(e => e.EntityId == entityId);

            query = AddDatePeriod(query, dateFrom, dateTo);

            return query.OrderByDescending(e => e.RegisteredAt).ToListAsync();
        }

        public async Task<List<LocationHistoryEntity>> GetLocationHistoryAsync(IReadOnlyCollection<Guid> entityIdList, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = Context.LocationHistory.Where(lh => lh.EntityId != null && entityIdList.Contains(lh.EntityId.Value));

            query = AddDatePeriod(query, dateFrom, dateTo);

            return await Context.LocationHistory
                .Where(lh => lh.EntityId != null && entityIdList.Contains(lh.EntityId.Value))
                .OrderByDescending(lh => lh.RegisteredAt)
                .ToListAsync();
        }

        private static IQueryable<LocationHistoryEntity> AddDatePeriod(IQueryable<LocationHistoryEntity> query, DateTime? dateFrom, DateTime? dateTo)
        {
            if(dateFrom.HasValue)
            {
                query = query.Where(e => e.RegisteredAt >= dateFrom);
            }

            if(dateTo.HasValue)
            {
                query = query.Where(e => e.RegisteredAt <= dateTo);
            }
            return query;
        }
    }
}
