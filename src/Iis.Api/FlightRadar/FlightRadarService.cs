using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Iis.DataModel.FlightRadar;
using Iis.DbLayer.Repositories;
using Iis.Domain.FlightRadar;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData;
using IIS.Repository;
using IIS.Repository.Factories;

namespace IIS.Core.FlightRadar
{
    public class FlightRadarService<TUnitOfWork> : BaseService<TUnitOfWork>, IFlightRadarService where TUnitOfWork : IIISUnitOfWork
    {
        private const string SignName = "ICAOSign";
        private readonly IMapper _mapper;
        private readonly IOntologySchema _ontologySchema;
        private OntologyNodesData _ontologyNodesData;

        public FlightRadarService(IMapper mapper,
            IOntologySchema ontologySchema,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory) : base(unitOfWorkFactory)
        {
            _mapper = mapper;
            _ontologySchema = ontologySchema;

            SignalSynchronizationStart();
        }

        public void SignalSynchronizationStart()
        {
            if (_ontologyNodesData == null)
            {
                var nodes = RunWithoutCommit(unitOfWork => unitOfWork.OntologyRepository.GetAllNodes());
                var relations = RunWithoutCommit(unitOfWork => unitOfWork.OntologyRepository.GetAllRelations());
                var attributes = RunWithoutCommit(unitOfWork => unitOfWork.OntologyRepository.GetAllAttributes());
                var rawData = new NodesRawData(nodes, relations, attributes);
                _ontologyNodesData = new OntologyNodesData(rawData, _ontologySchema);
            }
        }

        public async Task SaveFlightRadarDataAsync(string icao, IReadOnlyCollection<FlightRadarHistory> historyItems)
        {
            var signs = GetIcaoSigns(icao);

            if (!signs.Any())
            {
                return;
            }

            var entityIds = GetIncomingEntities(signs);
            await SaveHistoryEntities(icao, historyItems, entityIds);

        }

        private IEnumerable<Guid> GetIncomingEntities(IEnumerable<INode> signs)
        {
            return signs.SelectMany(p => p.IncomingRelations.Where(r => r.IsLinkToSeparateObject).Select(r => r.SourceNodeId));
        }

        private async Task SaveHistoryEntities(string icao, IReadOnlyCollection<FlightRadarHistory> historyItems, IEnumerable<Guid> entityIds)
        {
            var histotyEntities = new List<FlightRadarHistoryEntity>();
            foreach (var entityId in entityIds)
            {
                histotyEntities.AddRange(
                    _mapper.Map<List<FlightRadarHistoryEntity>>(historyItems)
                    .Select(p =>
                    {
                        p.Id = Guid.NewGuid();
                        p.NodeId = entityId;
                        p.ICAO = icao;
                        return p;
                    }));
            }
            await RunAsync(async unitOfWork => await unitOfWork.FlightRadarRepository.SaveAsync(histotyEntities));
        }

        private IEnumerable<INode> GetIcaoSigns(string icao)
        {
            return _ontologyNodesData.GetEntitiesByTypeName(SignName).Where(p => NodeHasPropertyWithValue(p, "value", icao));
        }

        private bool NodeHasPropertyWithValue(INode node, string propertyName, string value)
        {
            return node.OutgoingRelations.Any(r => r.TypeName == propertyName && r.TargetNode.Value == value);
        }

        public Task UpdateLastProcessedIdAsync(FlightRadarHistorySyncJobConfig minId, int newMinId)
        {
            Run(unitOfWork => unitOfWork.FlightRadarRepository.RemoveSyncJobConfig());
            var configToAdd = new FlightRadarHistorySyncJobConfig()
            {
                LatestProcessedId = newMinId
            };
            return RunAsync(async unitOfWork => await unitOfWork.FlightRadarRepository.AddSyncJobConfigAsync(configToAdd));
        }

        public async Task<FlightRadarHistorySyncJobConfig> GetLastProcessedIdAsync()
        {
            return await RunWithoutCommitAsync(async unitOfWork => await unitOfWork.FlightRadarRepository.GetLastProcessedIdAsync());
        }

        public void SignalSynchronizationStop()
        {
            _ontologyNodesData = null;
        }
    }
}
