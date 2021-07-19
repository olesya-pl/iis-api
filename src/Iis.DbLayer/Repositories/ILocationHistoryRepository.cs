using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.DataModel.FlightRadar;

namespace Iis.DbLayer.Repositories
{
    public interface ILocationHistoryRepository
    {
        Task<LocationHistoryEntity> GetLatestLocationHistoryEntityAsync(Guid entityId);
        Task<LocationHistoryEntity[]> GetLatestLocationHistoryListAsync(IReadOnlyCollection<Guid> entityIdList);
        Task<LocationHistoryEntity[]> GetLocationHistoryEntityListByMaterialIdAsync(Guid materialId);
        Task SaveAsync(IReadOnlyCollection<LocationHistoryEntity> entityList);
    }
}