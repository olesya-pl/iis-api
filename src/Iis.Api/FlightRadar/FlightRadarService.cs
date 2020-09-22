using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Iis.DataModel.FlightRadar;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Domain.FlightRadar;
using IIS.Repository;
using IIS.Repository.Factories;

namespace IIS.Core.FlightRadar
{
    public class FlightRadarService<TUnitOfWork> : BaseService<TUnitOfWork>, IFlightRadarService where TUnitOfWork : IIISUnitOfWork
    {
        private const string SignName = "ICAOSign";
        private readonly IMapper _mapper;
        private readonly IOntologyModel _ontologyModel;
        private readonly IOntologyService _ontologyService;

        public FlightRadarService(IMapper mapper,
            IOntologyModel ontologyModel,
            IOntologyService ontologyService,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory) : base(unitOfWorkFactory)
        {
            _mapper = mapper;
            _ontologyModel = ontologyModel;
            _ontologyService = ontologyService;
        }

        public async Task SaveFlightRadarDataAsync(string icao, IReadOnlyCollection<FlightRadarHistory> historyItems)
        {
            var signs = (await GetIcaoSigns(icao)).Select(p => p.EntityId).ToList();

            if (!signs.Any())
            {
                return;
            }

            var entityRelations = await _ontologyService.GetIncomingEntities(signs);

            if (!entityRelations.Any())
            {
                return;
            }

            await SaveHistoryEntities(icao, historyItems, entityRelations);
        }

        private Task SaveHistoryEntities(string icao, IReadOnlyCollection<FlightRadarHistory> historyItems, List<IncomingRelation> entityRelations)
        {
            var histotyEntities = new List<FlightRadarHistoryEntity>();
            foreach (var relation in entityRelations)
            {
                histotyEntities.AddRange(
                    _mapper.Map<List<FlightRadarHistoryEntity>>(historyItems)
                    .Select(p =>
                    {
                        p.NodeId = relation.EntityId;
                        p.ICAO = icao;
                        return p;
                    }));
            }
            return RunAsync(async unitOfWork => await unitOfWork.FlightRadarRepository.SaveAsync(histotyEntities));
        }

        private async Task<IEnumerable<IncomingRelation>> GetIcaoSigns(string icao)
        {
            var icaoSignType = _ontologyModel.GetEntityType(SignName);
            if (icaoSignType == null)
            {
                return Enumerable.Empty<IncomingRelation>();
            }
            var nodes = await _ontologyService.GetNodesByUniqueValue(icaoSignType.Id, icao, "value", int.MaxValue);
            var nodeIds = nodes.Select(p => p.Id).ToList();
            return await _ontologyService.GetIncomingEntities(nodeIds);
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
    }
}
