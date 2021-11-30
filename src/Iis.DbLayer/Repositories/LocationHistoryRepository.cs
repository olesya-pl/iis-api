using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Iis.DataModel;
using Iis.DataModel.FlightRadar;
using IIS.Repository;
using Iis.DbLayer.DirectQueries;
using Iis.Interfaces.DirectQueries;

namespace Iis.DbLayer.Repositories
{
    public class LocationHistoryRepository : RepositoryBase<OntologyContext>, ILocationHistoryRepository
    {
        private readonly IDirectQueryFactory _directQueryFactory;
        public LocationHistoryRepository(IDirectQueryFactory directQueryFactory)
        {
            _directQueryFactory = directQueryFactory;
        }
        public Task<LocationHistoryEntity> GetLatestLocationHistoryEntityAsync(Guid entityId)
        {
            return Context.LocationHistory
                    .OrderByDescending(e => e.RegisteredAt)
                    .FirstOrDefaultAsync(e => e.EntityId == entityId);
        }

        public Task<LocationHistoryEntity[]> GetLatestLocationHistoryListAsync(IReadOnlyCollection<Guid> nodeTypeIds)
        {
            var listValue = string.Join(',', nodeTypeIds.Select(e => $"'{e.ToString("N")}'"));
            var query = _directQueryFactory.GetDirectQuery(DirectQueryTypes.LocationHistoryLast)
                .SetParameter("NodeTypeIds", listValue)
                .GetFinalSql();

            return Context.LocationHistory.FromSqlRaw(query).ToArrayAsync();
        }

        public Task<LocationHistoryEntity[]> GetLocationHistoryEntityListByMaterialIdAsync(Guid materialId)
        {
            return Context.LocationHistory
                    .OrderByDescending(e => e.RegisteredAt)
                    .Where(e => e.MaterialId == materialId)
                    .ToArrayAsync();
        }

        public Task SaveAsync(IReadOnlyCollection<LocationHistoryEntity> entityList)
        {
            return Context.AddRangeAsync(entityList);
        }
    }
}