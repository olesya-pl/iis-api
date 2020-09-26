using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.DataModel.FlightRadar;
using Iis.Domain.FlightRadar;

namespace IIS.Core.FlightRadar
{
    public interface IFlightRadarService
    {
        Task SaveFlightRadarDataAsync(string icao, IReadOnlyCollection<FlightRadarHistory> historyItems);
        Task UpdateLastProcessedIdAsync(FlightRadarHistorySyncJobConfig minId, int newMinId);
        Task<FlightRadarHistorySyncJobConfig> GetLastProcessedIdAsync();
        void SignalSynchronizationStart();
        void SignalSynchronizationStop();
    }
}