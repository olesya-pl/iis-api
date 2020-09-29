using System.Collections.Generic;
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
    }
}
