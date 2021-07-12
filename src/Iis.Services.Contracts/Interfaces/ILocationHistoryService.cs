using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Services.Contracts.Dtos;
namespace Iis.Services.Contracts.Interfaces
{
    public interface ILocationHistoryService
    {
        Task<LocationHistoryDto> GetLatestLocationHistoryAsync(Guid entityId);
        Task SaveLocationHistoryAsync(LocationHistoryDto locationHistoryDto);
        Task SaveLocationHistoryAsync(IReadOnlyCollection<LocationHistoryDto> locationHistoryList);
    }
}