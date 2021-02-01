using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Iis.DataModel.FlightRadar;
using Iis.DbLayer.Repositories;
using Iis.Domain;
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
        private readonly IOntologyService _ontologyService;
        private readonly IOntologySchema _ontologySchema;
        private IOntologyNodesData _ontologyData;

        public FlightRadarService(IMapper mapper,
            IOntologySchema ontologySchema,
            IOntologyService ontologyService,
            IOntologyNodesData ontologyData,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory) : base(unitOfWorkFactory)
        {
            _mapper = mapper;
            _ontologyService = ontologyService;
            _ontologySchema = ontologySchema;
            _ontologyData = ontologyData;
        }
        public async Task SaveFlightRadarDataAsync(string icao, IReadOnlyCollection<FlightRadarHistory> historyItems)
        {
            var signs = GetIcaoSigns(icao);

            if (!signs.Any())
            {
                return;
            }

            var signEntityRelations = GetIncomingEntities(signs);
            SaveSignsLocations(signs, historyItems);
            await SaveHistoryEntitiesAsync(historyItems, signEntityRelations);

        }

        private void SaveSignsLocations(IEnumerable<INode> signs, IReadOnlyCollection<FlightRadarHistory> historyItems)
        {
            var latestValue = historyItems.OrderBy(p => p.RegisteredAt).LastOrDefault();
            if (latestValue is null)
                return;
            foreach (var sign in signs)
            {
                var node = (_ontologyService.GetNode(sign.Id)) as Entity;
                var registeredAt = node.GetAttributeValue("registeredAt");
                if (registeredAt is null || (registeredAt is DateTime && (DateTime)registeredAt < latestValue.RegisteredAt))
                {
                    node.SetProperty("location", new Dictionary<string, object> {
                        { "type", "Point" },
                        { "coordinates", new [] {latestValue.Lat, latestValue.Long} }
                    });
                    node.SetProperty("registeredAt", latestValue.RegisteredAt);
                    _ontologyService.SaveNode(node);
                }                
            }
        }

        private IEnumerable<SignEntityRelation> GetIncomingEntities(IEnumerable<INode> signs)
        {
            return signs
                .SelectMany(p => p.IncomingRelations.Where(r => r.IsLinkToSeparateObject)
                .Select(r => new SignEntityRelation(r.TargetNodeId, r.SourceNodeId)));
        }

        private async Task SaveHistoryEntitiesAsync(IReadOnlyCollection<FlightRadarHistory> historyItems, IEnumerable<SignEntityRelation> signEntityRelations)
        {
            var histotyEntities = new List<LocationHistoryEntity>();
            foreach (var relation in signEntityRelations)
            {
                histotyEntities.AddRange(
                    _mapper.Map<List<LocationHistoryEntity>>(historyItems)
                    .Select(p =>
                    {
                        p.Id = Guid.NewGuid();
                        p.EntityId = relation.EntityId;
                        p.NodeId = relation.NodeId;
                        return p;
                    }));
            }
            await RunAsync(async unitOfWork => await unitOfWork.FlightRadarRepository.SaveAsync(histotyEntities));
        }

        private IEnumerable<INode> GetIcaoSigns(string icao)
        {
            return _ontologyData.GetEntitiesByTypeName(SignName).Where(p => p.HasPropertyWithValue("value", icao));
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

        private class SignEntityRelation
        {
            public SignEntityRelation(Guid nodeId, Guid entityId)
            {
                NodeId = nodeId;
                EntityId = entityId;
            }

            public Guid NodeId { get; set; }
            public Guid EntityId { get; set; }
        }
    }
}
