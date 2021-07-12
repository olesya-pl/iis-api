using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Iis.DataModel;
using Iis.DataModel.FlightRadar;
using IIS.Repository;

namespace Iis.DbLayer.Repositories
{
    public class LocationHistoryRepository : RepositoryBase<OntologyContext>, ILocationHistoryRepository
    {
        public Task<LocationHistoryEntity> GetLatestLocationHistoryEntityAsync(Guid entityId)
        {
            return Context.LocationHistory
                    .OrderByDescending(e => e.RegisteredAt)
                    .FirstOrDefaultAsync(e => e.EntityId == entityId);
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