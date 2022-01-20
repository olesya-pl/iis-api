using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using Iis.DataModel.FlightRadar;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Domain.FlightRadar;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using IIS.Repository;
using IIS.Repository.Factories;
using Microsoft.Extensions.Logging;

namespace IIS.Core.FlightRadar
{
    public class FlightRadarService<TUnitOfWork> : BaseService<TUnitOfWork>, IFlightRadarService where TUnitOfWork : IIISUnitOfWork
    {
        private const string SignTypeName = "ICAOSign";
        private const string SignTypePropName = "value";
        private const string RegisteredAtPropName = "registeredAt";
        private const string LocationPropName = "location";
        private const string TypePropName = "type";
        private const string CoordinatesPropName = "coordinates";
        private const string SignIdSeparator = ", ";
        private readonly IMapper _mapper;
        private readonly ILogger<FlightRadarService<TUnitOfWork>> _logger;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologySchema _ontologySchema;
        private readonly IOntologyNodesData _ontologyData;
        private readonly INodeTypeLinked _icaoSignNodeType;

        public FlightRadarService(IMapper mapper,
            ILogger<FlightRadarService<TUnitOfWork>> logger,
            IOntologySchema ontologySchema,
            IOntologyService ontologyService,
            IOntologyNodesData ontologyData,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory) : base(unitOfWorkFactory)
        {
            _mapper = mapper;
            _logger = logger;
            _ontologyService = ontologyService;
            _ontologySchema = ontologySchema;
            _ontologyData = ontologyData;

            _icaoSignNodeType = _ontologySchema.GetEntityTypeByName(SignTypeName);

        }

        public async Task SaveFlightRadarDataAsync(string icaoValue, IReadOnlyCollection<FlightRadarHistory> historyItemList)
        {
            var signList = GetSignEntityListByValue(icaoValue);

            if (!signList.Any())
            {
                _logger.LogInformation($"Icao '{icaoValue}' handling skiped.");
                return;
            }

            var latestLocation = GetLatestLocation(historyItemList);
            UpdateSignListWithLocation(signList, latestLocation);

            await AddSignListLocationHistoryAsync(signList, historyItemList);

            var signIds = string.Join(SignIdSeparator, signList.Select(_ => _.Id.ToString("N")));
            _logger.LogInformation($"Icao '{icaoValue}' related to signs: {signIds}.");
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

        private FlightRadarHistory GetLatestLocation(IReadOnlyCollection<FlightRadarHistory> historyItemList)
        {
            return historyItemList.OrderBy(p => p.RegisteredAt).LastOrDefault();
        }

        private void UpdateSignListWithLocation(IReadOnlyCollection<INode> signList, FlightRadarHistory newLocation)
        {
            if (newLocation is null) return;

            foreach (var sign in signList)
            {
                var signNode = _ontologyService.GetNode(sign.Id) as Entity;
                var registeredAtValue = signNode.GetAttributeValue(RegisteredAtPropName);

                if (IsNewLocationIsLessRecent(registeredAtValue, newLocation.RegisteredAt)) continue;

                signNode.SetProperty(RegisteredAtPropName, newLocation.RegisteredAt);
                signNode.SetProperty(LocationPropName, new Dictionary<string, object>
                {
                    {TypePropName, "Point"},
                    {CoordinatesPropName, new []{newLocation.Lat, newLocation.Long}}
                });
                _ontologyService.SaveNode(signNode);
            }
        }

        private bool IsNewLocationIsLessRecent(object currentRegisteredAt, DateTime newRegisteredAt)
        {
            if (currentRegisteredAt != null && currentRegisteredAt is DateTime registeredAt && registeredAt > newRegisteredAt) return true;
            return false;
        }

        private Task AddSignListLocationHistoryAsync(IReadOnlyCollection<INode> signList, IReadOnlyCollection<FlightRadarHistory> historyItemList)
        {
            var historyEntityList = signList.SelectMany(sign =>
                _mapper.Map<LocationHistoryEntity[]>(historyItemList)
                    .Select(p => {
                        p.Id = Guid.NewGuid();
                        p.EntityId = sign.Id;
                        p.NodeId = sign.Id;
                        return p;
                    })
            ).ToArray();

            return RunAsync(unitOfWork => unitOfWork.FlightRadarRepository.SaveAsync(historyEntityList));
        }

        private IReadOnlyCollection<INode> GetSignEntityListByValue(string icaoValue)
        {
            return _ontologyData
                        .GetNodesByUniqueValue(_icaoSignNodeType.Id, icaoValue, SignTypePropName)
                        .ToArray();
        }
    }
}