using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Mappers.RadioElectronicSituation;
using Iis.DbLayer.Repositories;
using IIS.Repository;
using IIS.Repository.Factories;
using Iis.DataModel.Materials;
using Iis.DataModel.FlightRadar;

namespace Iis.Services
{
    public class RadioElectronicSituationService<TUnitOfWork> : BaseService<TUnitOfWork>, IRadioElectronicSituationService where TUnitOfWork : IIISUnitOfWork
    {
        private const int HistoryAllocationMultiplier = 10;
        private static readonly string[] ObjectSignTypeNames = new[] { EntityTypeNames.ObjectSign.ToString() };
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
                                .ToDictionary(e => e.Id);

            var signIds = signsDictionary
                                .Select(_ => _.Key)
                                .Distinct()
                                .ToArray();

            var locationHistories = await RunWithoutCommitAsync(_ => _.LocationHistoryRepository.GetLatestLocationHistoryListAsync(signIds));

            if (locationHistories.Length == 0) return Array.Empty<SituationNodeDto>();

            var materialIdCollection = locationHistories
                                        .Where(_ => _.MaterialId.HasValue)
                                        .Select(_ => _.MaterialId.Value)
                                        .ToHashSet();

            var materialCollection = await RunWithoutCommitAsync(_ => _.MaterialRepository.GetByIdsAsync(materialIdCollection));

            var materialDictionary = materialCollection
                                        .ToDictionary(_ => _.Id);

            var mappingData = new List<(LocationHistoryEntity LocationHistory, INode SignNode, INode ObjectNode, MaterialEntity Material)>(locationHistories.Length * HistoryAllocationMultiplier);

            foreach (var locationHistory in locationHistories)
            {
                if(!signsDictionary.TryGetValue(locationHistory.EntityId.Value, out INode signNode)) continue;

                var material = GetMaterialEntity(locationHistory.MaterialId, materialDictionary);

                var data = signNode
                    .GetIncomingRelations(SignProperties)
                    .Select(p => (LocationHistory: locationHistory, SignNode: signNode, ObjectNode: p.SourceNode, Material: material))
                    .ToArray();

                mappingData.AddRange(data);
            }

            return mappingData
                    .Select(_ => RadioElectronicSituationMapper.Map(
                        _.LocationHistory,
                        _.SignNode,
                        _.ObjectNode,
                        _.Material
                    ))
                    .ToArray();
        }

        private MaterialEntity GetMaterialEntity(Guid? materialId, Dictionary<Guid, MaterialEntity> materialDictionary)
        {
            return materialId.HasValue ? materialDictionary.GetValueOrDefault(materialId.Value) : null;
        }
    }
}