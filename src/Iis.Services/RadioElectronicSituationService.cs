using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.Ontology.Comparers;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Mappers.RadioElectronicSituation;
using Iis.DbLayer.Repositories;
using Iis.Utility;
using IIS.Repository;
using IIS.Repository.Factories;
using Iis.DataModel.Materials;
using Iis.DataModel.FlightRadar;

namespace Iis.Services
{
    public class RadioElectronicSituationService<TUnitOfWork> : BaseService<TUnitOfWork>, IRadioElectronicSituationService where TUnitOfWork : IIISUnitOfWork
    {
        private const int CoordinateClusterPrecision = 3;
        private const int HistoryAllocationMultiplier = 10;
        private static readonly string[] ObjectSignTypeNames = new[] { "SatelliteIridiumPhoneSign", "SatellitePhoneSign", "CellphoneSign" };
        private static readonly string[] SignProperties = new[] { "sign" };
        private readonly IOntologyNodesData _data;
        private readonly IOntologySchema _schema;
        public RadioElectronicSituationService(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            IOntologyNodesData data,
            IOntologySchema schema)
        : base(unitOfWorkFactory)
        {
            _data = data;
            _schema = schema;
        }

        public async Task<IReadOnlyCollection<SituationNodeDto>> GetSituationNodesAsync()
        {
            var signTypes = _schema
                                .GetEntityTypesByName(ObjectSignTypeNames, true)
                                .Select(_ => _.Id)
                                .ToArray();

            var signsDictionary = _data.GetNodesByTypeIds(signTypes)
                                .Distinct(NodeByIdComparer.Instance)
                                .ToDictionary(e => e.Id);

            var signIds = signsDictionary
                                .Select(_ => _.Key)
                                .ToArray();

            var locationHistories = await RunWithoutCommitAsync(_ => _.LocationHistoryRepository.GetLatestLocationHistoryListAsync(signIds));

            locationHistories = locationHistories
                                .Where(_ => signIds.Contains(_.EntityId.Value))
                                .ToArray();

            if (locationHistories.Length == 0) return Array.Empty<SituationNodeDto>();

            var materialIdCollection = locationHistories
                                        .Where(_ => _.MaterialId.HasValue)
                                        .Select(_ => _.MaterialId.Value)
                                        .ToHashSet();

            var materialCollection = await RunWithoutCommitAsync(_ => _.MaterialRepository.GetByIdsAsync(materialIdCollection));

            var materialDictionary = materialCollection
                                        .ToDictionary(_ => _.Id);

            var locationClusterCollection = ClusterizeLocations(locationHistories);

            var result = new List<SituationNodeDto>(locationClusterCollection.Count);

            foreach (var point in locationClusterCollection)
            {
                var signDtoList = new List<SignDto>(point.Value.Length);
                var materialDtoList = new List<MaterialDto>(point.Value.Length);
                var objectDtoList = new List<ObjectDto>(point.Value.Length * HistoryAllocationMultiplier);

                foreach (var locationEntity in point.Value)
                {
                    if (!signsDictionary.TryGetValue(locationEntity.EntityId.Value, out INode signNode)) continue;

                    var materialEntity = GetMaterialEntity(locationEntity.MaterialId, materialDictionary);

                    var objectCollection = signNode
                                            .GetIncomingRelations(SignProperties)
                                            .Select(_ => RadioElectronicSituationMapper.MapObjectOfStudy(_.SourceNode, signNode, materialEntity))
                                            .ToArray();

                    if (objectCollection.Length == 0) continue;

                    signDtoList.Add(RadioElectronicSituationMapper.MapObjectSign(signNode, materialEntity));

                    if (materialEntity != null)
                    {
                        materialDtoList.Add(RadioElectronicSituationMapper.MapMaterial(materialEntity));
                    }

                    objectDtoList.AddRange(objectCollection);
                }

                if (objectDtoList.Count == 0) continue;

                objectDtoList = objectDtoList
                                .OrderByDescending(_ => _.MaterialRegistrationDate)
                                .ThenBy(_ => _.Id)
                                .ToList();

                var attributes = new AttributesDto(objectDtoList, signDtoList, materialDtoList);


                result.Add(new SituationNodeDto(point.Key, attributes));
            }

            return result.AsReadOnly();
        }

        private MaterialEntity GetMaterialEntity(Guid? materialId, Dictionary<Guid, MaterialEntity> materialDictionary)
        {
            return materialId.HasValue ? materialDictionary.GetValueOrDefault(materialId.Value) : null;
        }

        private IReadOnlyDictionary<GeometryDto, LocationHistoryEntity[]> ClusterizeLocations(IReadOnlyCollection<LocationHistoryEntity> locationCollection)
        {
            var roundedLocationCollection = locationCollection
                                            .Select(_ => (Key: new GeometryDto(_.Lat.Truncate(CoordinateClusterPrecision), _.Long.Truncate(CoordinateClusterPrecision)), Location: _))
                                            .ToArray();

            return roundedLocationCollection
                            .GroupBy(_ => _.Key)
                            .ToDictionary(_ => _.Key, _ => _.Select(_ => _.Location).ToArray());
        }
    }
}