using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.DataModel.FlightRadar;

namespace Iis.DbLayer.Repositories
{
    public interface IFlightRadarRepository
    {
        Task SaveAsync(IReadOnlyCollection<FlightRadarHistoryEntity> entity);
        void RemoveSyncJobConfig();
        Task AddSyncJobConfigAsync(FlightRadarHistorySyncJobConfig configToAdd);
        Task<FlightRadarHistorySyncJobConfig> GetLastProcessedIdAsync();
    }
}
