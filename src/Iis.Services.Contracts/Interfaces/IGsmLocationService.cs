using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using Iis.Services.Contracts.Dtos;
namespace Iis.Services.Contracts.Interfaces
{
    public interface IGsmLocationService
    {
        Task TryFillTowerLocationHistory(JObject metadata, Guid materialId);
        Task<IReadOnlyCollection<LocationHistoryDto>> GetLocationHistoryCollectionAsync(JObject metadata, Guid materialId);
    }
}