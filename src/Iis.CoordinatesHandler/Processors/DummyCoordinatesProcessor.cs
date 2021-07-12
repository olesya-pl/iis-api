using System;
using System.Threading.Tasks;
using Iis.Domain.Materials;
using Iis.Services.Contracts.Dtos;

namespace Iis.CoordinatesEventHandler.Processors
{
    public class DummyCoordinatesProcessor : ICoordinatesProcessor
    {
        public bool IsDummy => true;

        public Task<LocationHistoryDto[]> GetLocationHistoryListAsync(Material material)
        {
            return Task.FromResult(Array.Empty<LocationHistoryDto>());
        }
    }
}