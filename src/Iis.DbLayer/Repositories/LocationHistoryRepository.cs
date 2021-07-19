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

        public Task<LocationHistoryEntity[]> GetLatestLocationHistoryListAsync(IReadOnlyCollection<Guid> entityIdList)
        {
            var listValue = string.Join(',', entityIdList.Select(e => $"'{e.ToString("N")}'"));

            var query = "select \"Id\", \"Lat\", \"Long\", \"RegisteredAt\", \"NodeId\", \"EntityId\", \"ExternalId\", \"MaterialId\", \"Type\" from public.\"LocationHistory\" lh "+
                        "join ( select distinct first_value(\"Id\") over (partition by \"EntityId\" order by \"RegisteredAt\" desc) as \"ID\" FROM public.\"LocationHistory\" "+
                        "where \"EntityId\" in ("+listValue+")) jlh on lh.\"Id\" = jlh.\"ID\"";

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