using System.Threading.Tasks;
using Iis.Domain.Materials;
using Iis.Services.Contracts.Dtos;

namespace Iis.CoordinatesEventHandler.Processors
{
    public interface ICoordinatesProcessor
    {
        bool IsDummy { get; }
        Task<LocationHistoryDto[]> GetLocationHistoryListAsync(Material material);
    }
}