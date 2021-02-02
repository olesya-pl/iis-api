﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.DataModel.FlightRadar;

namespace Iis.DbLayer.Repositories
{
    public interface IFlightRadarRepository
    {
        Task SaveAsync(IReadOnlyCollection<LocationHistoryEntity> entity);
        void RemoveSyncJobConfig();
        Task AddSyncJobConfigAsync(FlightRadarHistorySyncJobConfig configToAdd);
        Task<FlightRadarHistorySyncJobConfig> GetLastProcessedIdAsync();
        Task<List<LocationHistoryEntity>> GetLocationHistory(Guid entityId);
        Task<List<LocationHistoryEntity>> GetLocationHistory(Guid entityId, DateTime? dateFrom, DateTime? dateTo);
    }
}
