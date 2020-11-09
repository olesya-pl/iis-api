using Iis.DataModel.Reports;
using Iis.DbLayer.Repositories;
using Iis.Elastic;
using Iis.Elastic.ElasticMappingProperties;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Enums;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Services
{
    public class AdminOntologyElasticService : IAdminOntologyElasticService
    {
        private readonly IElasticManager _elasticManager;
        private readonly IElasticState _elasticState;
        private readonly IOntologySchema _ontologySchema;
        private readonly INodeRepository _nodeRepository;
        private readonly IOntologyNodesData _ontologyNodesData;
        private readonly IReportElasticService _reportElasticService;
        private readonly IReportService _reportService;
        private readonly IAliasService _aliasService;

        private readonly Dictionary<AliasType, string> _elasticIndexByAliasType;

        public StringBuilder Logger { get; set; }

        public AdminOntologyElasticService(
            IElasticState elasticState,
            IElasticManager elasticManager,
            IOntologySchema ontologySchema,
            INodeRepository nodeRepository,
            IOntologyNodesData ontologyNodesData,
            IReportElasticService reportElasticService,
            IReportService reportService, 
            IAliasService aliasService)
        {
            _elasticState = elasticState ?? throw new ArgumentNullException(nameof(elasticState));
            _elasticManager = elasticManager ?? throw new ArgumentNullException(nameof(elasticManager));
            _ontologySchema = ontologySchema ?? throw new ArgumentNullException(nameof(ontologySchema));
            _nodeRepository = nodeRepository ?? throw new ArgumentNullException(nameof(nodeRepository));
            _ontologyNodesData = ontologyNodesData ?? throw new ArgumentNullException(nameof(ontologyNodesData));
            _reportElasticService = reportElasticService ?? throw new ArgumentNullException(nameof(reportElasticService));
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _aliasService = aliasService ?? throw new ArgumentNullException(nameof(aliasService));

            _elasticIndexByAliasType = new Dictionary<AliasType, string>()
            {
                { AliasType.Material, _elasticState.MaterialIndexes.First() }
            };
        }

        public bool IsIndexesValid(IEnumerable<string> indexes)
        {
            var notValidIndexes = indexes
                          .Where(name => !_elasticState.OntologyIndexes.Contains(name))
                          .ToList();

            if (notValidIndexes.Any())
            {
                TryLog($"There are not valid index names in list: {string.Join(", ", notValidIndexes)}");
                return false;
            }

            return true;
        }

        public async Task DeleteIndexesAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default)
        {
            var indexesToDelete = isHistorical ? GetHistoricalIndexes(indexes) : indexes;
            await DeleteIndexesAsync(indexesToDelete);
        }

        public async Task CreateIndexWithMappingsAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default)
        {
            foreach (var index in indexes)
            {
                ct.ThrowIfCancellationRequested();

                var attributesInfo = isHistorical
                    ? _ontologySchema.GetHistoricalAttributesInfo(index, _elasticState.HistoricalOntologyIndexes[index])
                    : _ontologySchema.GetAttributesInfo(index);

                var result = await _elasticManager.CreateMapping(attributesInfo, ct);

                if (!result)
                    TryLog($"mapping was not created for {index}");
            }
        }

        public async Task FillIndexesFromMemoryAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default)
        {
            var nodes = GetNodesFromMemory(indexes);
            var response = isHistorical
                ? await _nodeRepository.PutHistoricalNodesAsync(nodes, ct)
                : await _nodeRepository.PutNodesAsync(nodes, ct);

            LogBulkResponse(response);
        }

        public async Task DeleteIndexesAsync(IEnumerable<string> indexes, CancellationToken ct = default)
        {
            foreach (var index in indexes)
            {
                ct.ThrowIfCancellationRequested();
                var result = await _elasticManager.DeleteIndexAsync(index, ct);
                if (!result)
                    TryLog($"{index} was not deleted");
            }
        }

        public async Task CreateReportIndexWithMappingsAsync(CancellationToken ct = default)
        {
            var mappingConfiguration =  new ElasticMappingConfiguration(new List<ElasticMappingProperty> {
                KeywordProperty.Create(nameof(ReportEntity.Id), false),
                TextProperty.Create(nameof(ReportEntity.Recipient), null),
                TextProperty.Create(nameof(ReportEntity.Title), null),
                DateProperty.Create(nameof(ReportEntity.CreatedAt), ElasticConfiguration.DefaultDateFormats),
                KeywordProperty.Create("ReportEventIds", true)
            });

            await _elasticManager.CreateIndexesAsync(new[] { _elasticState.ReportIndex }, mappingConfiguration.ToJObject(), ct);
        }

        public async Task FillReportIndexAsync(CancellationToken ct = default)
        {
            var reports = await _reportService.GetAllAsync();
            
            TryLog($"Found {reports.Count} reports");

            var response = await _reportElasticService.PutAsync(reports);
            LogBulkResponse(response);
        }

        public async Task AddAliasesToIndexAsync(AliasType type, CancellationToken ct = default) 
        {
            if (!_elasticIndexByAliasType.ContainsKey(type))
                throw new NotImplementedException($"{type} is not supported");

            var index = _elasticIndexByAliasType[type];
            var aliasProperties = (await _aliasService.GetByTypeAsync(type))
                .Select(x => AliasProperty.Create(x.Value, x.DotName))
                .ToList();

            foreach (var property in aliasProperties)
            {
                ct.ThrowIfCancellationRequested();

                var aliasMappingConfiguration = new ElasticMappingConfiguration(new List<ElasticMappingProperty> { property });
                var elasticResponse = await _elasticManager.AddMappingPropertyToIndexAsync(index, aliasMappingConfiguration.GetPropertiesJObject(), ct);
                if (!elasticResponse.IsSuccess)
                    TryLog($"{property.Name} was not created due to {elasticResponse.ErrorType}: {elasticResponse.ErrorReason}");
            }
        }

        private List<INode> GetNodesFromMemory(IEnumerable<string> indexes)
        {
            var nodes = new List<INode>();
            foreach (var index in indexes)
            {
                var type = _ontologySchema.GetEntityTypeByName(index);
                var entities = _ontologyNodesData.GetEntitiesByTypeName(type.Name);
                nodes.AddRange(entities);
            }

            return nodes;
        }

        private void TryLog(string message)
        {
            if (Logger != null)
                Logger.AppendLine(message);

            return;
        }

        private void LogBulkResponse(IEnumerable<ElasticBulkResponse> response)
        {
            var successResponses = response.Where(x => x.IsSuccess);
            TryLog($"Success operations: {successResponses.Count()}");
            foreach (var item in successResponses.GroupBy(x => x.SuccessOperation))
            {
                TryLog($"nodes were {item.Key}: {item.Count()}");
            }

            var failedRespones = response.Where(x => !x.IsSuccess);
            TryLog($"Failed operations: {failedRespones.Count()}");
            foreach (var group in failedRespones.GroupBy(x => x.Id))
            {
                TryLog($"error occurred for Id:{group.Key}, errorType:{group.First().ErrorType}, error message:{group.First().ErrorReason}");
            }
        }

        private List<string> GetHistoricalIndexes(IEnumerable<string> indexes)
        {
            return indexes
                .Select(x => _elasticState.HistoricalOntologyIndexes.GetValueOrDefault(x))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
        }
    }
}
