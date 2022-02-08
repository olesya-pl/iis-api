using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core;
using Iis.DbLayer.Repositories;
using Iis.Elastic;
using Iis.Elastic.ElasticMappingProperties;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Enums;
using Iis.OntologyData.IisAccessLevels;
using Iis.Services;
using Iis.Services.Contracts.Csv;
using Iis.Services.Contracts.Interfaces;
using IIS.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MoreLinq;
using Iis.Services.Contracts.Elastic;
using Iis.Utility;
using Iis.Elastic.Entities;
using Iis.Interfaces.SecurityLevels;
using Newtonsoft.Json;

namespace Iis.Api.Controllers
{
    [Route("{controller}")]
    [ApiController]
    public class AdminController : Controller
    {
        private const string AllIndexes = "all";
        private readonly IMaterialElasticService _materialElasticService;
        private readonly IElasticManager _elasticManager;
        private readonly INodeSaveService _nodeSaveService;
        private readonly IElasticState _elasticState;
        private readonly IUserService _userService;
        private readonly IUserElasticService _userElasticService;
        private readonly IAdminOntologyElasticService _adminElasticService;
        private readonly IHost _host;
        private readonly IOntologyDataService _nodesDataService;
        private readonly IConnectionStringService _connectionStringService;
        private readonly IAccessLevelService _accessLevelService;
        private readonly ICsvService _csvService;
        private readonly IMaterialProvider _materialProvider;
        private readonly NodeMaterialRelationService<IIISUnitOfWork> _nodeMaterialRelationService;
        private readonly ISecurityLevelChecker _securityLevelChecker;

        public AdminController(
            IMaterialElasticService materialElasticService,
            IElasticManager elasticManager,
            INodeSaveService nodeSaveService,
            IElasticState elasticState,
            IUserService userService,
            IUserElasticService userElasticService,
            IAdminOntologyElasticService adminElasticService,
            IHost host,
            IConnectionStringService connectionStringService,
            IOntologyDataService nodesDataService,
            IAccessLevelService accessLevelService,
            ICsvService csvService,
            IMaterialProvider materialProvider,
            NodeMaterialRelationService<IIISUnitOfWork> nodeMaterialRelationService,
            ISecurityLevelChecker securityLevelChecker)
        {
            _materialElasticService = materialElasticService;
            _elasticManager = elasticManager;
            _nodeSaveService = nodeSaveService;
            _elasticState = elasticState;
            _adminElasticService = adminElasticService;
            _userService = userService;
            _userElasticService = userElasticService;
            _host = host;
            _nodesDataService = nodesDataService;
            _connectionStringService = connectionStringService;
            _accessLevelService = accessLevelService;
            _csvService = csvService;
            _materialProvider = materialProvider;
            _nodeMaterialRelationService = nodeMaterialRelationService;
            _securityLevelChecker = securityLevelChecker;
        }

        [HttpGet("CreateBindingNodesToMaterial/{nodeId}/{limit}")]
        public async Task<string> CreateBindingNodesToMaterial(Guid nodeId, int limit)
        {
            var materialsIds = await _materialProvider.GetMaterialsIdsAsync(limit);

            await _nodeMaterialRelationService.CreateMultipleRelations(
                new HashSet<Guid>(new[] { nodeId }), new HashSet<Guid>(materialsIds), null);

            return nodeId.ToString();
        }

        [HttpGet("ReInitializeOntologyIndexes/{indexNames}")]
        public Task<IActionResult> ReInitializeOntologyIndexes(string indexNames, CancellationToken cancellationToken)
        {
            return CreateOntologyIndexes(indexNames, _elasticState.OntologyIndexes, cancellationToken);
        }

        [HttpGet("RemoveHistoricalIndexes")]
        public async Task<IActionResult> RemoveHistoricalIndexesAsync(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var allIndexes = _elasticState.WikiIndexes.Union(_elasticState.OntologyIndexes);

            _adminElasticService.Logger = new StringBuilder();

            await _adminElasticService.DeleteHistoricalIndexesAsync(allIndexes, cancellationToken);

            _adminElasticService.Logger.AppendLine($"spend: {stopwatch.ElapsedMilliseconds} ms");

            return Content(_adminElasticService.Logger.ToString());
        }

        [HttpGet("ReInitializeWikiIndexes/{indexNames}")]
        public Task<IActionResult> ReInitializeWikiIndexes(string indexNames, CancellationToken cancellationToken)
        {
            return CreateOntologyIndexes(indexNames, _elasticState.WikiIndexes, cancellationToken);
        }

        [HttpGet("ReInitializeSignIndexes/{indexNames}")]
        public Task<IActionResult> ReInitializeSignIndexes(string indexNames, CancellationToken cancellationToken)
        {
            return CreateOntologyIndexes(indexNames, _elasticState.SignIndexes, cancellationToken);
        }

        [HttpGet("ReInitializeEventIndexes")]
        public async Task<IActionResult> ReInitializeEventIndexes(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            _adminElasticService.Logger = new StringBuilder();

            var indexes = _elasticState.EventIndexes;
            await _adminElasticService.DeleteIndexesAsync(indexes, cancellationToken);

            await _adminElasticService.CreateIndexWithMappingsAsync(indexes, cancellationToken);

            if (_elasticState.FieldsToExcludeByIndex.TryGetValue(indexes.First(), out var fieldsToExclude))
            {
                await _adminElasticService.FillIndexesFromMemoryAsync(indexes, fieldsToExclude, cancellationToken);
            }
            else
            {
                await _adminElasticService.FillIndexesFromMemoryAsync(indexes, cancellationToken);
            }

            _adminElasticService.Logger.AppendLine($"spend: {stopwatch.ElapsedMilliseconds} ms");

            return Content(_adminElasticService.Logger.ToString());
        }

        [HttpGet("RecreateElasticReportIndex")]
        public async Task<IActionResult> RecreateReportIndex(CancellationToken cancellationToken)
        {
            _adminElasticService.Logger = new StringBuilder();
            var index = _elasticState.ReportIndex;

            await _adminElasticService.DeleteIndexesAsync(index.AsArray(), cancellationToken);
            await _adminElasticService.CreateReportIndexWithMappingsAsync(cancellationToken);
            await _adminElasticService.FillReportIndexAsync(cancellationToken);

            return Content(_adminElasticService.Logger.ToString());
        }

        [HttpGet("RecreateElasticMaterialIndexes")]
        public async Task<IActionResult> RecreateElasticMaterialIndexes(CancellationToken cancellationToken)
        {
            var log = new StringBuilder();
            _adminElasticService.Logger = log;

            var materialIndex = _elasticState.MaterialIndexes.First();

            await _elasticManager.DeleteIndexAsync(materialIndex, cancellationToken);

            var mappingConfiguration = new ElasticMappingConfiguration(new List<ElasticMappingProperty>
            {
                ByteProperty.Create("SecurityAttributes.AccessLevel"),
                ByteProperty.Create("AccessLevel"),
                TextProperty.Create("Content", ElasticConfiguration.DefaultTermVector),
                KeywordProperty.Create("Metadata.features.PhoneNumber", false),
                DateProperty.Create("Metadata.RegTime", ElasticConfiguration.DefaultDateFormats),
                DateProperty.Create("Metadata.RegDate", ElasticConfiguration.DefaultDateFormats),
                DateProperty.Create("RegistrationDate", ElasticConfiguration.DefaultDateFormats),
                IntegerProperty.Create("Metadata.Duration"),
                DateProperty.Create("CreatedDate", ElasticConfiguration.DefaultDateFormats),
                DateProperty.Create("UpdatedAt", ElasticConfiguration.DefaultDateFormats),
                DateProperty.Create("LoadData.ReceivingDate", ElasticConfiguration.DefaultDateFormats),
                KeywordProperty.Create("ParentId", true),
                TextProperty.Create("Type", true),
                TextProperty.Create("Source", true),
                AliasProperty.Create(MaterialAliases.Type.Alias, MaterialAliases.Type.Path),
                AliasProperty.Create(MaterialAliases.Source.Alias, MaterialAliases.Source.Path),
                KeywordProperty.Create(MaterialAliases.ProcessedStatus.Path, false),
                AliasProperty.Create(MaterialAliases.ProcessedStatus.Alias, MaterialAliases.ProcessedStatus.Path),
                ByteProperty.Create("ProcessedStatus.OrderNumber"),
                DateProperty.Create("ProcessedAt", ElasticConfiguration.DefaultDateFormats),
                KeywordProperty.Create(MaterialAliases.Completeness.Path, false),
                AliasProperty.Create(MaterialAliases.Completeness.Alias, MaterialAliases.Completeness.Path),
                KeywordProperty.Create(MaterialAliases.Importance.Path, false),
                AliasProperty.Create(MaterialAliases.Importance.Alias, MaterialAliases.Importance.Path),
                ByteProperty.Create("Importance.OrderNumber"),
                KeywordProperty.Create(MaterialAliases.Reliability.Path, false),
                AliasProperty.Create(MaterialAliases.Reliability.Alias, MaterialAliases.Reliability.Path),
                KeywordProperty.Create(MaterialAliases.Relevance.Path, false),
                AliasProperty.Create(MaterialAliases.Relevance.Alias, MaterialAliases.Relevance.Path),
                KeywordProperty.Create(MaterialAliases.SourceReliability.Path, false),
                AliasProperty.Create(MaterialAliases.SourceReliability.Alias, MaterialAliases.SourceReliability.Path),
                KeywordProperty.Create(MaterialAliases.SessionPriority.Path, false),
                AliasProperty.Create(MaterialAliases.SessionPriority.Alias, MaterialAliases.SessionPriority.Path),
                ByteProperty.Create("SessionPriority.OrderNumber"),
                IntegerProperty.Create("NodesCount"),
                DenseVectorProperty.Create("ImageVectors.Vector", MaterialDocument.ImageVectorDimensionsCount),
                TextProperty.Create("MLResponses.namedEntityRecognition", ElasticConfiguration.DefaultTermVector),
                TextProperty.Create("MLResponses.textAnnotation", ElasticConfiguration.DefaultTermVector),
                KeywordProperty.Create(nameof(MaterialDocument.Channel), false),
                KeywordProperty.Create(MaterialAliases.Assignees.Path, false),
                AliasProperty.Create(MaterialAliases.Assignees.Alias, MaterialAliases.Assignees.Path),
                KeywordProperty.Create(MaterialAliases.SecurityLevels.Path, true),
            });
            await _elasticManager.CreateIndexesAsync(
                new[] { materialIndex },
                mappingConfiguration.ToJObject(),
                cancellationToken);

            var response = await _materialElasticService.PutAllMaterialsToElasticSearchAsync(cancellationToken);

            await _adminElasticService.AddAliasesToIndexAsync(AliasType.Material, cancellationToken);

            LogElasticResult(log, response);

            return Content(log.ToString());
        }

        [HttpGet("RecreateElasticChangeHistoryIndexes")]
        public async Task<IActionResult> RecreateElasticChangeHistoryIndexes(CancellationToken cancellationToken)
        {
            var log = new StringBuilder();
            _adminElasticService.Logger = log;

            var index = _elasticState.ChangeHistoryIndexes.First();

            await _elasticManager.DeleteIndexAsync(index, cancellationToken);

            var mappingConfiguration = new ElasticMappingConfiguration(new List<ElasticMappingProperty>
            {
                KeywordProperty.Create("TargetId", false),
                KeywordProperty.Create("RequestId", false),
                KeywordProperty.Create("UserName", true),
                KeywordProperty.Create("PropertyName", true),
                DateProperty.Create("Date", ElasticConfiguration.DefaultDateFormats),
                TextProperty.Create("OldValue", true),
                TextProperty.Create("NewValue", true),
                IntegerProperty.Create("Type"),
                TextProperty.Create("ParentTypeName", true),
                TextProperty.Create("OldTitle", true),
                TextProperty.Create("NewTitle", true),
                KeywordProperty.Create("Roles.Id", false),
                KeywordProperty.Create("Roles.Name", false),
                AliasProperty.Create("Користувач", "UserName"),
                AliasProperty.Create("Назва поля", "PropertyName"),
                AliasProperty.Create("Дата зміни", "Date"),
                AliasProperty.Create("Було", "OldValue"),
                AliasProperty.Create("Стало", "NewValue")
            });
            await _elasticManager.CreateIndexesAsync(
                index.AsArray(),
                mappingConfiguration.ToJObject(),
                cancellationToken);

            var response = await _materialElasticService.PutAllMaterialChangesToElasticSearchAsync(cancellationToken);

            LogElasticResult(log, response);

            return Content(log.ToString());
        }

        [HttpGet("RecreateElasticUserIndexes")]
        public async Task<IActionResult> RecreateElasticUserIndexes(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            var log = new StringBuilder();
            _adminElasticService.Logger = log;

            var indexSecurityParam = new List<(IReadOnlyCollection<string>, string)>
            {
                (_elasticState.MaterialIndexes, "AccessLevel"),
                (_elasticState.ReportIndex.AsArray(), "AccessLevel"),
                (_elasticState.OntologyIndexes, "__accessLevel"),
                (_elasticState.WikiIndexes, "__accessLevel"),
                (_elasticState.EventIndexes, "__accessLevel"),
            };
            await _elasticManager.CreateSecurityMappingAsync(indexSecurityParam, cancellationToken);

            log.AppendLine("Role created");
            await _userElasticService.ClearNonPredefinedUsers(cancellationToken);
            await _userService.PutAllUsersToElasticSearchAsync(cancellationToken);
            log.AppendLine("Users created");

            log.AppendLine($"spend: {stopwatch.ElapsedMilliseconds} ms");

            return Content(log.ToString());
        }

        [HttpGet("ReInitializeSecurityIndexes")]
        public async Task<IActionResult> ReInitializeSecurityIndexesAsync(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            var log = new StringBuilder();
            _adminElasticService.Logger = log;

            var index = _elasticState.SecurityIndexes.First();

            await _elasticManager.DeleteIndexAsync(index, cancellationToken);

            var securityLevelsPlain = _securityLevelChecker.GetSecurityLevelsPlain();
            foreach (var level in securityLevelsPlain)
            {
                var json = JsonConvert.SerializeObject(level);
                await _elasticManager.PutDocumentAsync(index, level.Id, json, cancellationToken);
            }
            log.AppendLine($"Security indexes created. Spend: {stopwatch.ElapsedMilliseconds} ms");

            return Content(log.ToString());
        }

        [HttpGet("GetElasticJson/{id}")]
        public async Task<IActionResult> GetElasticJson(string id, CancellationToken cancellationToken)
        {
            var uid = new Guid(id);
            var jObj = await _nodeSaveService.GetJsonNodeByIdAsync(uid, cancellationToken);
            if (jObj == null)
            {
                return Content($"Entity is not found for id = {uid}");
            }
            var json = jObj.ToString(Newtonsoft.Json.Formatting.Indented);
            return Content(json);
        }

        [HttpPost("RestartApplication")]
        public async Task RestartApplication()
        {
            Program.NeedToStart = true;
            await _host.StopAsync();
        }

        [HttpPost("ReloadOntologyData")]
        public Task<IActionResult> ReloadOntologyData()
        {
            var connectionString = _connectionStringService.GetIisApiConnectionString();
            _nodesDataService.ReloadOntologyData(connectionString);
            return Task.FromResult<IActionResult>(Content("Success"));
        }

        [HttpPost("ChangeAccessLevels")]
        public async Task ChangeAccessLevels(ChangeAccessLevelsParams param, CancellationToken cancellationToken)
        {
            var newAccessLevels = new AccessLevels(param.AccessLevelList);
            await _accessLevelService.ChangeAccessLevels(newAccessLevels, param.DeletedMappings, cancellationToken);
            await ReInitializeOntologyIndexes("all", cancellationToken);
        }

        [HttpGet("ImportExternalUsers/{userNames}")]
        public async Task<IActionResult> ImportExternalUsers(string userNames, CancellationToken cancellationToken)
        {
            string message;
            try
            {
                message = await _userService.ImportUsersFromExternalSourceAsync(userNames.Split(','), cancellationToken);
            }
            catch (Exception ex)
            {
                message = $"Error: {ex.Message}";
            }
            return Content(message);
        }

        [HttpGet("ImportExternalUsers")]
        public async Task<IActionResult> ImportExternalUsers(CancellationToken cancellationToken)
        {
            string message;
            try
            {
                message = await _userService.ImportUsersFromExternalSourceAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                message = $"Error: {ex.Message}";
            }
            return Content(message);
        }

        [HttpGet("CheckMatrixUsers")]
        public async Task<IActionResult> CheckMatrixUsers()
        {
            var result = await _userService.GetUserMatrixInfoAsync();
            return Content(result);
        }

        [HttpGet("CreateMatrixUsers")]
        public async Task<IActionResult> CreateMatrixUsers()
        {
            var result = await _userService.CreateMatrixUsersAsync();
            return Content(result);
        }

        [HttpGet("GetCsv/{typeName}")]
        public Task<IActionResult> GetCsv(string typeName)
        {
            var result = _csvService.GetDorCsvByTypeName(typeName);
            var bytes = Encoding.Unicode.GetBytes(result);
            var csv = Encoding.Unicode.GetPreamble().Concat(bytes).ToArray();

            return Task.FromResult<IActionResult>(File(csv, "text/csv", $"{typeName}.csv"));
        }

        private static void LogElasticResult(StringBuilder log, IEnumerable<ElasticBulkResponse> response)
        {
            var successResponses = response.Where(x => x.IsSuccess);
            log.AppendLine($"Success operations: {successResponses.Count()}");
            foreach (var item in successResponses.GroupBy(x => x.SuccessOperation))
            {
                log.AppendLine($"{item.Key}: {item.Count()}");
            }

            var failedRespones = response.Where(x => !x.IsSuccess);
            log.AppendLine($"Failed operations: {failedRespones.Count()}");
            foreach (var group in failedRespones.GroupBy(x => x.Id))
            {
                log.AppendLine($"error occurred for Id:{group.Key}, errorType:{group.First().ErrorType}, error message:{group.First().ErrorReason}");
            }
        }

        private async Task<IActionResult> CreateOntologyIndexes(
            string indexNames,
            IReadOnlyCollection<string> baseIndexList,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            var indexes = indexNames == AllIndexes ? baseIndexList : indexNames.Split(",");

            var notValidIndexes = indexes
                .Where(name => !baseIndexList.Contains(name, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if (notValidIndexes.Any())
            {
                return Content($"There are not valid index names in list: {string.Join(", ", notValidIndexes)}");
            }

            _adminElasticService.Logger = new StringBuilder();

            await _adminElasticService.DeleteIndexesAsync(indexes, cancellationToken);

            await _adminElasticService.CreateIndexWithMappingsAsync(indexes, cancellationToken);

            await _adminElasticService.FillIndexesFromMemoryAsync(indexes, cancellationToken);

            _adminElasticService.Logger.AppendLine($"spend: {stopwatch.ElapsedMilliseconds} ms");

            return Content(_adminElasticService.Logger.ToString());
        }
    }
}