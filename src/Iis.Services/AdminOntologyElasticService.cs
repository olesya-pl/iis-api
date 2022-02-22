using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel.Reports;
using Iis.Elastic;
using Iis.Elastic.ElasticMappingProperties;
using Iis.Elastic.Entities;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Enums;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using Iis.Services.Contracts.Interfaces;
using Iis.Utility;

namespace Iis.Services
{
    public class AdminOntologyElasticService : IAdminOntologyElasticService
    {
        private const string LocationPropertyName = "location";
        private readonly IElasticManager _elasticManager;
        private readonly IElasticState _elasticState;
        private readonly IOntologySchema _ontologySchema;
        private readonly INodeSaveService _nodeRepository;
        private readonly IOntologyNodesData _ontologyNodesData;
        private readonly IReportElasticService _reportElasticService;
        private readonly IReportService _reportService;
        private readonly IAliasService _aliasService;

        private readonly Dictionary<AliasType, string> _elasticIndexByAliasType;
        private readonly IReadOnlyCollection<IAttributeInfoItem> _historicalAttributes = new IAttributeInfoItem[]
        {
            new AttributeInfoItem($"{ElasticSerializer.HistoricalPropertyName}.{nameof(ChangeHistoryDocument.Date)}", ScalarType.Date, "Дата зміни".AsArray(), false),
            new AttributeInfoItem($"{ElasticSerializer.HistoricalPropertyName}.{nameof(ChangeHistoryDocument.OldValue)}", ScalarType.String, "Було".AsArray(), true),
            new AttributeInfoItem($"{ElasticSerializer.HistoricalPropertyName}.{nameof(ChangeHistoryDocument.NewValue)}", ScalarType.String, "Стало".AsArray(), true),
            new AttributeInfoItem($"{ElasticSerializer.HistoricalPropertyName}.{nameof(ChangeHistoryDocument.OldTitle)}", ScalarType.String, Array.Empty<string>(), true),
            new AttributeInfoItem($"{ElasticSerializer.HistoricalPropertyName}.{nameof(ChangeHistoryDocument.NewTitle)}", ScalarType.String, Array.Empty<string>(), true),
            new AttributeInfoItem($"{ElasticSerializer.HistoricalPropertyName}.{nameof(ChangeHistoryDocument.ParentTypeName)}", ScalarType.String, Array.Empty<string>(), true),
            new AttributeInfoItem($"{ElasticSerializer.HistoricalPropertyName}.{nameof(ChangeHistoryDocument.PropertyName)}", ScalarType.String, "Назва поля".AsArray(), true),
            new AttributeInfoItem($"{ElasticSerializer.HistoricalPropertyName}.{nameof(ChangeHistoryDocument.UserName)}", ScalarType.String, "Користувач".AsArray(), true)
        };

        public StringBuilder Logger { get; set; }

        public AdminOntologyElasticService(
            IElasticState elasticState,
            IElasticManager elasticManager,
            IOntologySchema ontologySchema,
            INodeSaveService nodeRepository,
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

        public Task CreateIndexWithMappingsAsync(IReadOnlyCollection<string> indexes, CancellationToken cancellationToken = default)
        {
            return indexes.ForEachAsync(async index =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var attributesInfo = _ontologySchema.GetAttributesInfo(index);

                attributesInfo.AddItems(_historicalAttributes);

                await CreateIndexWithMappingsAsync(attributesInfo, index, cancellationToken);
            });
        }

        public async Task FillIndexesFromMemoryAsync(IEnumerable<string> indexes, CancellationToken cancellationToken = default)
        {
            var nodes = GetNodesFromMemory(indexes);
            var response = await _nodeRepository.PutNodesAsync(nodes, cancellationToken);

            LogBulkResponse(response);
        }

        public async Task FillIndexesFromMemoryAsync(IEnumerable<string> indexes, IEnumerable<string> fieldsToExclude, CancellationToken cancellationToken = default)
        {
            var nodes = GetNodesFromMemory(indexes);
            var response = await _nodeRepository.PutNodesAsync(nodes, fieldsToExclude, cancellationToken);

            LogBulkResponse(response);
        }

        public async Task DeleteIndexesAsync(IEnumerable<string> indexes, CancellationToken cancellationToken = default)
        {
            foreach (var index in indexes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await _elasticManager.DeleteIndexAsync(index, cancellationToken);
                if (!result)
                    TryLog($"{index} was not deleted");
            }
        }

        public Task DeleteHistoricalIndexesAsync(IEnumerable<string> indexes, CancellationToken cancellationToken = default)
        {
            var indexesToDelete = GetHistoricalIndexes(indexes);

            return DeleteIndexesAsync(indexesToDelete, cancellationToken);
        }

        public Task CreateReportIndexWithMappingsAsync(CancellationToken cancellationToken = default)
        {
            var mappingConfiguration = new ElasticMappingConfiguration(new List<ElasticMappingProperty>
            {
                KeywordProperty.Create(nameof(ReportEntity.Id), false),
                TextProperty.Create(nameof(ReportEntity.Recipient), true),
                TextProperty.Create(nameof(ReportEntity.Title), true),
                DateProperty.Create(nameof(ReportEntity.CreatedAt), ElasticConfiguration.DefaultDateFormats),
                KeywordProperty.Create("ReportEventIds", true)
            });

            return _elasticManager.CreateIndexesAsync(_elasticState.ReportIndex.AsArray(), mappingConfiguration.ToJObject(), cancellationToken);
        }

        public async Task FillReportIndexAsync(CancellationToken cancellationToken = default)
        {
            var reports = await _reportService.GetAllAsync();

            TryLog($"Found {reports.Count} reports");

            var response = await _reportElasticService.PutAsync(reports);
            LogBulkResponse(response);
        }

        public async Task AddAliasesToIndexAsync(AliasType type, CancellationToken cancellationToken = default)
        {
            if (!_elasticIndexByAliasType.ContainsKey(type))
                throw new NotImplementedException($"{type} is not supported");

            var index = _elasticIndexByAliasType[type];
            var aliasProperties = (await _aliasService.GetByTypeAsync(type))
                .Select(x => AliasProperty.Create(x.Value, x.DotName))
                .ToList();

            foreach (var property in aliasProperties)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var aliasMappingConfiguration = new ElasticMappingConfiguration(new List<ElasticMappingProperty> { property });
                var elasticResponse = await _elasticManager.AddMappingPropertyToIndexAsync(index, aliasMappingConfiguration.GetPropertiesJObject(), cancellationToken);
                if (!elasticResponse.IsSuccess)
                    TryLog($"{property.Name} was not created due to {elasticResponse.ErrorType}: {elasticResponse.ErrorReason}");
            }
        }

        public async Task CreateIndexWithMappingsAsync(IAttributeInfoList attributesInfo, string index, CancellationToken cancellationToken)
        {
            if (_ontologySchema.GetEntityTypeByName(index).IsObjectSign)
            {
                var singLocationAttribute = new AttributeInfoItem(LocationPropertyName, ScalarType.GeoPoint, null, false);
                attributesInfo.TryAddItem(singLocationAttribute);
            }

            var result = await _elasticManager.CreateMapping(attributesInfo, cancellationToken);

            if (!result)
                TryLog($"mapping was not created for {index}");
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

        private string GetHistoricalIndex(string typeName) => $"historical_{typeName}";

        private List<string> GetHistoricalIndexes(IEnumerable<string> indexes) => indexes.Select(x => GetHistoricalIndex(x)).ToList();
    }
}