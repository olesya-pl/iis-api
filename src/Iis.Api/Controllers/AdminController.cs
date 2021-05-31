using Iis.DataModel;
using Iis.DbLayer.OntologyData;
using Iis.DbLayer.Repositories;
using Iis.Elastic;
using Iis.Elastic.ElasticMappingProperties;
using Iis.Interfaces.AccessLevels;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Enums;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData;
using Iis.OntologyData.IisAccessLevels;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using IIS.Core;
using IIS.Core.Materials;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Api.Controllers
{
    [Route("{controller}")]
    [ApiController]
    public class AdminController : Controller
    {
        private const string AllIndexes = "all";
        private readonly IElasticManager _elasticManager;
        private readonly INodeRepository _nodeRepository;
        private readonly IMaterialService _materialService;
        private readonly IElasticState _elasticState;
        private readonly IUserService _userService;
        private readonly IUserElasticService _userElasticService;
        private readonly IAdminOntologyElasticService _adminElasticService;
        private readonly IHost _host;
        private readonly IConfiguration _configuration;
        private readonly IOntologyDataService _nodesDataService;
        private readonly IConnectionStringService _connectionStringService;
        private readonly IAccessLevelService _accessLevelService;

        public AdminController(
            IMaterialService materialService,
            IElasticManager elasticManager,
            INodeRepository nodeRepository,
            IElasticState elasticState,
            IUserService userService,
            IUserElasticService userElasticService,
            IAdminOntologyElasticService adminElasticService,
            IHost host,
            IConnectionStringService connectionStringService,
            IOntologyDataService nodesDataService,
            IAccessLevelService accessLevelService)
        {
            _elasticManager = elasticManager;
            _materialService = materialService;
            _nodeRepository = nodeRepository;
            _elasticState = elasticState;
            _adminElasticService = adminElasticService;
            _userService = userService;
            _userElasticService = userElasticService;
            _host = host;
            _nodesDataService = nodesDataService;
            _connectionStringService = connectionStringService;
            _accessLevelService = accessLevelService;
        }

        [HttpGet("ReInitializeOntologyIndexes/{indexNames}")]
        public Task<IActionResult> ReInitializeOntologyIndexes(string indexNames, CancellationToken ct)
        {
            return CreateOntologyIndexes(indexNames, _elasticState.OntologyIndexes, false, ct);
        }

        [HttpGet("ReInitializeHistoricalOntologyIndexes/{indexNames}")]
        public Task<IActionResult> ReInitializeHistoricalOntologyIndexes(string indexNames, CancellationToken ct)
        {
            return CreateOntologyIndexes(indexNames, _elasticState.OntologyIndexes, true, ct);
        }

        [HttpGet("ReInitializeWikiIndexes/{indexNames}")]
        public Task<IActionResult> ReInitializeWikiIndexes(string indexNames, CancellationToken ct)
        {
            return CreateOntologyIndexes(indexNames, _elasticState.WikiIndexes, false, ct);
        }

        [HttpGet("ReInitializeHistoricalWikiIndexes/{indexNames}")]
        public Task<IActionResult> ReInitializeHistoricalWikiIndexes(string indexNames, CancellationToken ct)
        {
            return CreateOntologyIndexes(indexNames, _elasticState.WikiIndexes, true, ct);
        }

        [HttpGet("ReInitializeSignIndexes/{indexNames}")]
        public Task<IActionResult> ReInitializeSignIndexes(string indexNames, CancellationToken ct)
        {
            return CreateOntologyIndexes(indexNames, _elasticState.SignIndexes, false, ct);
        }

        [HttpGet("ReInitializeEventIndexes")]
        public async Task<IActionResult> ReInitializeEventIndexes(CancellationToken ct)
        {
            var stopwatch = Stopwatch.StartNew();

            _adminElasticService.Logger = new StringBuilder();

            var indexes = _elasticState.EventIndexes;
            await _adminElasticService.DeleteIndexesAsync(indexes, false, ct);

            await _adminElasticService.CreateIndexWithMappingsAsync(indexes, false, ct);

            if (_elasticState.FieldsToExcludeByIndex.TryGetValue(indexes.First(), out var fieldsToExclude))
            {
                await _adminElasticService.FillIndexesFromMemoryAsync(indexes, fieldsToExclude, ct);
            }
            else
            {
                await _adminElasticService.FillIndexesFromMemoryAsync(indexes, false, ct);
            }

            _adminElasticService.Logger.AppendLine($"spend: {stopwatch.ElapsedMilliseconds} ms");

            return Content(_adminElasticService.Logger.ToString());

        }

        [HttpGet("RecreateElasticReportIndex")]
        public async Task<IActionResult> RecreateReportIndex(CancellationToken ct)
        {
            _adminElasticService.Logger = new StringBuilder();
            var index = _elasticState.ReportIndex;

            await _adminElasticService.DeleteIndexesAsync(new string[] { index }, ct);
            await _adminElasticService.CreateReportIndexWithMappingsAsync(ct);
            await _adminElasticService.FillReportIndexAsync(ct);

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
                TextProperty.Create("Metadata.Duration"),
                DateProperty.Create("CreatedDate", ElasticConfiguration.DefaultDateFormats),
                DateProperty.Create("LoadData.ReceivingDate", ElasticConfiguration.DefaultDateFormats),
                KeywordProperty.Create("ParentId", true),
                TextProperty.Create("Type", true),
                TextProperty.Create("Source", true),
                KeywordProperty.Create("ProcessedStatus.Title", false),
                ByteProperty.Create("ProcessedStatus.OrderNumber"),
                KeywordProperty.Create("Completeness.Title", false),
                KeywordProperty.Create("Importance.Title", false),
                ByteProperty.Create("Importance.OrderNumber"),
                KeywordProperty.Create("Reliability.Title", false),
                KeywordProperty.Create("Relevance.Title", false),
                KeywordProperty.Create("SourceReliability.Title", false),
                KeywordProperty.Create("SessionPriority.Title", false),
                ByteProperty.Create("SessionPriority.OrderNumber"),
                IntegerProperty.Create("NodesCount"),
                DenseVectorProperty.Create("ImageVectors.Vector", MaterialDocument.ImageVectorDimensionsCount)
            });

            await _elasticManager.CreateIndexesAsync(new[] { materialIndex },
                mappingConfiguration.ToJObject(),
                cancellationToken);

            var response = await _materialService.PutAllMaterialsToElasticSearchAsync(cancellationToken);

            await _adminElasticService.AddAliasesToIndexAsync(AliasType.Material, cancellationToken);

            LogElasticResult(log, response);

            return Content(log.ToString());
        }

        [HttpGet("RecreateElasticUserIndexes")]
        public async Task<IActionResult> RecreateElasticUserIndexes(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            var log = new StringBuilder();
            _adminElasticService.Logger = log;

            var indexSecurityParam = new List<(IReadOnlyCollection<string>, string)>{
                (_elasticState.MaterialIndexes, "AccessLevel"),
                (new [] { _elasticState.ReportIndex }, "AccessLevel"),
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

        [HttpGet("GetElasticJson/{id}")]
        public async Task<IActionResult> GetElasticJson(string id, CancellationToken cancellationToken)
        {
            var uid = new Guid(id);
            var jObj = await _nodeRepository.GetJsonNodeByIdAsync(uid, cancellationToken);
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
        public async Task<IActionResult> ReloadOntologyData()
        {
            var connectionString = _connectionStringService.GetIisApiConnectionString();
            _nodesDataService.ReloadOntologyData(connectionString);
            return Content("Success");
        }

        [HttpPost("ChangeAccessLevels")]
        public async Task ChangeAccessLevels(ChangeAccessLevelsParams param, CancellationToken ct)
        {
            var newAccessLevels = new AccessLevels(param.AccessLevelList);
            await _accessLevelService.ChangeAccessLevels(newAccessLevels, param.DeletedMappings, ct);
            await ReInitializeOntologyIndexes("all", ct);
        }

        [HttpPost("RemoveMaterials")]
        public async Task<IActionResult> RemoveMaterials()
        {
            await _materialService.RemoveMaterials();
            return Ok();
        }

        [HttpGet("ExportUsers/{userNames}")]
        public async Task<IActionResult> ExportUsers(string userNames, CancellationToken ct)
        {
            var cnt = _userService.ImportUsersFromExternalSource(userNames.Split(','));

            return Content($"{cnt} users where successfully imported");
        }

        [HttpGet("ExportUsers")]
        public async Task<IActionResult> ExportUsers(CancellationToken ct)
        {
            var cnt = _userService.ImportUsersFromExternalSource();

            return Content($"{cnt} users where successfully imported");
        }

        [HttpGet("CheckMatrixUsers")]
        public async Task<IActionResult> CheckMatrixUsers()
        {
            var result = await _userService.GetUserMatrixInfo();
            return Content(result);
        }

        [HttpGet("CreateMatrixUsers")]
        public async Task<IActionResult> CreateMatrixUsers()
        {
            var result = await _userService.CreateMatrixUsers();
            return Content(result);
        }

        private void LogElasticResult(StringBuilder log, IEnumerable<ElasticBulkResponse> response)
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

        private async Task<IActionResult> CreateOntologyIndexes(string indexNames, IReadOnlyCollection<string> baseIndexList, bool isHistorical, CancellationToken ct)
        {
            var stopwatch = Stopwatch.StartNew();

            var indexes = indexNames == AllIndexes ? baseIndexList : indexNames.Split(",");

            var notValidIndexes = indexes.Where(name => !baseIndexList.Contains(name, StringComparer.OrdinalIgnoreCase)).ToArray();

            if (notValidIndexes.Any())
            {
                return Content($"There are not valid index names in list: {string.Join(", ", notValidIndexes)}");
            }

            _adminElasticService.Logger  = new StringBuilder();

            await _adminElasticService.DeleteIndexesAsync(indexes, isHistorical, ct);

            await _adminElasticService.CreateIndexWithMappingsAsync(indexes, isHistorical, ct);

            await _adminElasticService.FillIndexesFromMemoryAsync(indexes, isHistorical, ct);

            _adminElasticService.Logger.AppendLine($"spend: {stopwatch.ElapsedMilliseconds} ms");

            return Content(_adminElasticService.Logger.ToString());
        }
    }
}
