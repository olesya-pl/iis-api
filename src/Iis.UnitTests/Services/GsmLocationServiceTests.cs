using System;
using System.Linq;
using Iis.DataModel;
using Iis.DataModel.FlightRadar;
using Iis.Services.Contracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Iis.UnitTests.Services
{
    public class GsmLocationServiceTests
    {
        private ServiceProvider _serviceProvider;

        public GsmLocationServiceTests()
        {
            _serviceProvider = Utils.GetServiceProvider();
        }

        public void Dispose()
        {
            CleanDatabase();
            _serviceProvider.Dispose();
        }

        [Fact]
        public void TryFillTowerLocationHistory_GivenTwoTowerLocations_ShouldCreateOneLocationHistory()
        {
            //Arrange
            SetDefaultTowerLocations(
                new TowerLocationEntity
                {
                    Id = 1,
                    Mcc = "251",
                    Mnc = "99",
                    Lac = "25010",
                    CellId = "4476",
                    Lat = 21.01233121m,
                    Long = 52.2138913m
                },
                new TowerLocationEntity
                {
                    Id = 2,
                    Mcc = "248",
                    Mnc = "456",
                    Lac = "912",
                    CellId = "12343",
                    Lat = 61.086781221m,
                    Long = 52.23242343m
                });
            var metadata = GetMetadata();
            var materialId = Guid.NewGuid();

            //Act
            var service = _serviceProvider.GetRequiredService<IGsmLocationService>();
            service.TryFillTowerLocationHistory(metadata, materialId);

            //Assert
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            var historicalLocations = context.LocationHistory.ToList();
            Assert.Single(historicalLocations);
            Assert.Equal(materialId, historicalLocations[0].MaterialId);
            Assert.Equal(21.01233121m, historicalLocations[0].Lat);
            Assert.Equal(52.2138913m, historicalLocations[0].Long);
            Assert.Equal(new DateTime(2018, 9, 26, 17, 29, 31), historicalLocations[0].RegisteredAt);
        }

        [Fact]
        public void TryFillTowerLocationHistory_GivenOneTowerLocations_ShouldNotCreateAnyLocationHistory()
        {
            //Arrange
            SetDefaultTowerLocations(
                new TowerLocationEntity
                {
                    Id = 1,
                    Mcc = "346",
                    Mnc = "99",
                    Lac = "25010",
                    CellId = "4476",
                    Lat = 21.01233121m,
                    Long = 52.2138913m
                });
            var metadata = GetMetadata();
            var materialId = Guid.NewGuid();

            //Act
            var service = _serviceProvider.GetRequiredService<IGsmLocationService>();
            service.TryFillTowerLocationHistory(metadata, materialId);

            //Assert
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            var historicalLocations = context.LocationHistory.ToList();
            Assert.Empty(historicalLocations);
        }

        [Fact]
        public void TryFillTowerLocationHistory_GivenInvalidMetadata_ShouldNotCreateAnyLocationHistory()
        {
            //Arrange
            SetDefaultTowerLocations(
                new TowerLocationEntity
                {
                    Id = 1,
                    Mcc = "346",
                    Mnc = "99",
                    Lac = "25010",
                    CellId = "4476",
                    Lat = 21.01233121m,
                    Long = 52.2138913m
                });
            var metadata = JObject.FromObject(new {name = "Petro"});
            var materialId = Guid.NewGuid();

            //Act
            var service = _serviceProvider.GetRequiredService<IGsmLocationService>();
            service.TryFillTowerLocationHistory(metadata, materialId);

            //Assert
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            var historicalLocations = context.LocationHistory.ToList();
            Assert.Empty(historicalLocations);
        }

        [Fact]
        public void TryFillTowerLocationHistory_GivenNoTowerLocation_ShouldNotCreateAnyLocationHistory()
        {
            //Arrange
            var metadata = GetMetadata();
            var materialId = Guid.NewGuid();

            //Act
            var service = _serviceProvider.GetRequiredService<IGsmLocationService>();
            service.TryFillTowerLocationHistory(metadata, materialId);

            //Assert
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            var historicalLocations = context.LocationHistory.ToList();
            Assert.Empty(historicalLocations);
        }

        private void CleanDatabase()
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            
            context.LocationHistory.RemoveRange(context.LocationHistory);
            context.TowerLocations.RemoveRange(context.TowerLocations);
            context.SaveChanges();
        }

        private JObject GetMetadata()
        {
            return JObject.Parse(@"{
            ""type"" : ""audio"",
            ""source"" : ""cell.voice"",
            ""RegTime"" : ""26.09.2018, 17:29:31"",
            ""CallType"" : ""Речь"",
            ""BTS_distance"" : ""1950"",
            ""Duration"" : ""25"",
            ""CallDirection"" : ""DW"",
            ""CallType"" : ""Речь"",
            ""Unit"" : ""Посейдон"",
            ""MCC"":""251"",
            ""MNC"":""99"",
            ""LAC"" : ""25010"",
            ""CellID"" : ""4476"",
            ""Region"" : ""Aqua"",
            ""Features"" : [
                {
                    ""featureId"" : ""AEF598C1-6200-42EC-A1BD-1351543DC884"",
                    ""PhoneNumber"" : ""380713032022"",
                    ""DBObject"" : """",
                    ""IMSI"" : """",
                    ""TMSI"" : ""A860AC66"",
                    ""IMEI"" : """"
                }
            ]
            }");
        }

        private void SetDefaultTowerLocations(params TowerLocationEntity[] entities)
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.TowerLocations.AddRange(entities);
            context.SaveChanges();
        }
    }
}