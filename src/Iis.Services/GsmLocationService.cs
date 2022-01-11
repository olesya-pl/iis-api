using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Iis.DataModel.FlightRadar;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Constants;
using Iis.Interfaces.Enums;
using IIS.Repository;
using IIS.Repository.Factories;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;

namespace Iis.Services
{
    public class GsmLocationService<TUnitOfWork> : BaseService<TUnitOfWork>, IGsmLocationService where TUnitOfWork : IIISUnitOfWork
    {
        private const string _dataDateFormatString = "dd.MM.yyyy, HH:mm:ss";
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
                DateFormatString = _dataDateFormatString,
        };

        public GsmLocationService(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory)
        : base(unitOfWorkFactory)
        {
        }

        public async Task TryFillTowerLocationHistory(JObject metadata, Guid materialId)
        {
            var model = metadata.ToObject<GsmMetadata>(JsonSerializer.Create(_jsonSerializerSettings));

            if (model == null || !CouldBeLocationExtracted(model))
                return;

            var (latitude, longitude) = await RunWithoutCommitAsync(_ => _.TowerLocationRepository.GetByCellGlobalIdentityAsync(model.Mcc, model.Mnc, model.Lac, model.CellId));

            if (latitude == default || longitude == default)
                return;

            await RunAsync(uow => uow.FlightRadarRepository.SaveAsync(new[] {new LocationHistoryEntity
                {
                    Lat = latitude,
                    Long = longitude,
                    RegisteredAt = model.RegTime,
                    NodeId = model.FeatureId,
                    MaterialId = materialId,
                    Type = LocationType.Material
                }
            }));
        }

        public async Task<IReadOnlyCollection<LocationHistoryDto>> GetLocationHistoryCollectionAsync(JObject metadata, Guid materialId)
        {
            var result = new List<LocationHistoryDto>();

            var model = metadata.ToObject<GsmMetadata>(JsonSerializer.Create(_jsonSerializerSettings));

            if (model == null || !CouldBeLocationExtracted(model)) return result;

            var (latitude, longitude) = await RunWithoutCommitAsync(_ => _.TowerLocationRepository.GetByCellGlobalIdentityAsync(model.Mcc, model.Mnc, model.Lac, model.CellId));

            if (latitude == default || longitude == default) return result;

            foreach (var feature in model.Features)
            {
                if(!feature.Id.HasValue || feature.Id == Guid.Empty) continue;

                result.Add(new LocationHistoryDto
                {
                    Lat = latitude,
                    Long = longitude,
                    RegisteredAt = model.RegTime,
                    EntityId = feature.Id,
                    NodeId = feature.Id,
                    MaterialId = materialId,
                    Type = LocationType.Material
                });
            }

            return result;
        }

        private static bool CouldBeLocationExtracted(GsmMetadata model)
        {
            return
                !string.IsNullOrEmpty(model.Mcc)
                && !string.IsNullOrEmpty(model.Mnc)
                && !string.IsNullOrEmpty(model.Lac)
                && !string.IsNullOrEmpty(model.CellId)
                && model.Features.Any(x => x.Id.HasValue);
        }

        private class Feature
        {
            [JsonProperty(FeatureFields.FeatureId)]
            public Guid? Id { get; set; }
        }

        private class GsmMetadata
        {
            [JsonProperty(FeatureFields.Mcc)] 
            public string Mcc { get; set; }

            [JsonProperty(FeatureFields.Mnc)] 
            public string Mnc { get; set; }

            [JsonProperty(FeatureFields.Lac)] 
            public string Lac { get; set; }

            [JsonProperty(FeatureFields.CellId)]
            public string CellId { get; set; }

            [JsonProperty(FeatureFields.RegTime)]
            public DateTime RegTime { get; set; }

            [JsonProperty(FeatureFields.FeaturesSection)]
            public List<Feature> Features { get; set; }

            [JsonIgnore]
            public Guid? FeatureId => Features.Select(x => x.Id).FirstOrDefault();
        }
    }
}