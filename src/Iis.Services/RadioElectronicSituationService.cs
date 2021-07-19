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

namespace Iis.Services
{
    public class RadioElectronicSituationService<TUnitOfWork> : BaseService<TUnitOfWork>, IRadioElectronicSituationService where TUnitOfWork : IIISUnitOfWork
    {
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
                                .Select(e => e.Id)
                                .ToArray();

            var signsDictionary = _data.GetNodesByTypeIds(signTypes)
                                .ToDictionary(e => e.Id);

            var signIds = signsDictionary
                                .Select(e => e.Key)
                                .ToArray();

            var locationHistories = await RunWithoutCommitAsync(uow => uow.LocationHistoryRepository.GetLatestLocationHistoryListAsync(signIds));

            if (locationHistories.Length == 0) return Array.Empty<SituationNodeDto>();

            var result = new List<SituationNodeDto>(locationHistories.Length);

            foreach (var locationHistory in locationHistories)
            {
                if(!signsDictionary.TryGetValue(locationHistory.EntityId.Value, out INode node)) continue;
                var entities = node.GetIncomingRelations(SignProperties)
                    .Select(p => p.SourceNode);

                foreach (var entity in entities)
                {
                    result.Add(RadioElectronicSituationMapper.Map(locationHistory, entity));
                }
            }

            return result;
        }
    }
}