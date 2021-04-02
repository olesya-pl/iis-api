using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel.FlightRadar;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Constants;
using IIS.Repository;
using IIS.Repository.Factories;
using Iis.Services.Contracts.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iis.Services
{
    public class GsmLocationService<TUnitOfWork> : BaseService<TUnitOfWork>, IGsmLocationService where TUnitOfWork : IIISUnitOfWork
    {
        private string _dataDateFormatString = "dd.MM.yyyy, HH:mm:ss";

        public GsmLocationService(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory) : base(unitOfWorkFactory)
        {
        }

        public async Task TryFillTowerLocationHistory(JObject metadata, Guid materialId)
        {
            var model = metadata.ToObject<GsmMetadata>(JsonSerializer.Create(new JsonSerializerSettings
            {   
                DateFormatString = _dataDateFormatString,
            }));

            if (model == null || !CouldBeLocationExtracted(model))
                return;

            var (lat, @long) = await RunWithoutCommitAsync(x =>
                x.TowerLocationRepository.GetByCellGlobalIdentityAsync(model.Mcc, model.Mnc, model.Lac, model.CellId));

            if (lat == default || @long == default)
                return;

            await RunAsync(uow => uow.FlightRadarRepository.SaveAsync(new[] {new LocationHistoryEntity
                {
                    Lat = lat,
                    Long = @long,
                    RegisteredAt = model.RegTime,
                    NodeId = model.FeatureId,
                    MaterialId = materialId,
                    Type = LocationType.Material
                }
            }));
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
            [JsonProperty(FeatureFields.featureId)]
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