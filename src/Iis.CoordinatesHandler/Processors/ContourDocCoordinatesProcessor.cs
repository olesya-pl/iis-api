using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.CoordinatesEventHandler.Types;
using Iis.Domain.Materials;
using Iis.Interfaces.Enums;
using Iis.Services.Contracts.Dtos;

namespace Iis.CoordinatesEventHandler.Processors
{
    public class ContourDocCoordinatesProcessor : ICoordinatesProcessor
    {
        private const string CoordinatesDivider = "_x000d__x000a_";
        public bool IsDummy => false;
        public Task<LocationHistoryDto[]> GetLocationHistoryListAsync(Material material)
        {
            var coordinatesValue = material.LoadData?.Coordinates;

            if (string.IsNullOrWhiteSpace(coordinatesValue)) return Task.FromResult(Array.Empty<LocationHistoryDto>());

            var result = new List<LocationHistoryDto>();
            var coordinatesArray = coordinatesValue.Split(CoordinatesDivider, StringSplitOptions.RemoveEmptyEntries);

            foreach (var coordinates in coordinatesArray)
            {
                var coordinate = new Coordinate(coordinates);

                if (!coordinate.IsValid) continue;

                var dto = new LocationHistoryDto
                {
                    MaterialId = material.Id,
                    Lat = coordinate.Latitude,
                    Long = coordinate.Longitude,
                    RegisteredAt = coordinate.RegisteredAt,
                    Type = LocationType.Material
                };

                result.Add(dto);
            }

            return Task.FromResult(result.ToArray());
        }
    }
}