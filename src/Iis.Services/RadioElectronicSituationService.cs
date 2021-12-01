using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.Ontology.Comparers;
using Iis.Services.Dictionaries;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Iis.DbLayer.Repositories;
using IIS.Repository;
using IIS.Repository.Factories;
using Iis.DataModel.Materials;
using Iis.DataModel.FlightRadar;
using Iis.Services.Contracts.Dtos.RadioElectronicSituation;
using AutoMapper;

namespace Iis.Services
{
    public class RadioElectronicSituationService<TUnitOfWork> : BaseService<TUnitOfWork>, IRadioElectronicSituationService where TUnitOfWork : IIISUnitOfWork
    {
        private const int HistoryAllocationMultiplier = 10;
        private static readonly string[] ObjectSignTypeNames = { SignTypeName.SatelliteIridiumPhone, SignTypeName.SatellitePhone, SignTypeName.CellPhone};
        private static readonly string[] SignProperties = { "sign" };
        private readonly IOntologyNodesData _data;
        private readonly IOntologySchema _schema;
        private readonly IMapper _mapper;
        public RadioElectronicSituationService(
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            IOntologyNodesData data,
            IOntologySchema schema,
            IMapper mapper)
        : base(unitOfWorkFactory)
        {
            _data = data;
            _schema = schema;
            _mapper = mapper;
        }

        public async Task<IReadOnlyCollection<ResSourceItemDto>> GetSituationNodesAsync()
        {
            var signTypes = _schema
                                .GetEntityTypesByName(ObjectSignTypeNames, true)
                                .Select(_ => _.Id)
                                .ToArray();

            var locationHistories = await RunWithoutCommitAsync(_ => _.LocationHistoryRepository.GetLatestLocationHistoryListAsync(signTypes));

            if (locationHistories.Length == 0) return Array.Empty<ResSourceItemDto>();

            var materialIdCollection = locationHistories
                                        .Where(_ => _.MaterialId.HasValue)
                                        .Select(_ => _.MaterialId.Value)
                                        .ToHashSet();

            var materialCollection = await RunWithoutCommitAsync(_ => _.MaterialRepository.GetByIdsAsync(materialIdCollection));

            var materialDictionary = materialCollection
                                        .ToDictionary(_ => _.Id);

            var mappingData = new List<ResSourceItemDto>(locationHistories.Length * HistoryAllocationMultiplier);

            foreach (var locationHistory in locationHistories)
            {
                var signNode = _data.GetNode(locationHistory.EntityId.Value);
                if (signNode == null) continue;

                var material = GetMaterialEntity(locationHistory.MaterialId, materialDictionary);

                var data = signNode
                    .GetIncomingRelations(SignProperties)
                    .Select(p => new ResSourceItemDto(
                        _mapper.Map<LocationHistoryDto>(locationHistory),
                        signNode,
                        p.SourceNode,
                        _mapper.Map<ResMaterialDto>(material)))
                    .ToArray();

                mappingData.AddRange(data);
            }

            return mappingData;
        }

        private static MaterialEntity GetMaterialEntity(Guid? materialId, Dictionary<Guid, MaterialEntity> materialDictionary)
        {
            return materialId.HasValue ? materialDictionary.GetValueOrDefault(materialId.Value) : null;
        }
    }
}