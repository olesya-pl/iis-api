using Iis.DbLayer.Repositories;
using Iis.Elastic;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.Materials;
using Microsoft.AspNetCore.Mvc;
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
        IElasticManager _elasticManager;
        INodeRepository _nodeRepository;
        IMaterialService _materialService;
        IElasticState _elasticState;
        private readonly IAdminOntologyElasticService _adminElasticService;
        
        public AdminController(
            IMaterialService materialService,
            IElasticManager elasticManager,
            INodeRepository nodeRepository,
            IElasticState elasticState,
            IAdminOntologyElasticService adminElasticService)
        {
            _elasticManager = elasticManager ?? throw new ArgumentNullException(nameof(elasticManager));
            _materialService = materialService ?? throw new ArgumentNullException(nameof(materialService));
            _nodeRepository = nodeRepository ?? throw new ArgumentNullException(nameof(nodeRepository));
            _elasticState = elasticState ?? throw new ArgumentNullException(nameof(elasticState));
            _adminElasticService = adminElasticService ?? throw new ArgumentNullException(nameof(adminElasticService));
        }

        [HttpPost("CreateHistoricalIndexes/{indexNames}")]
        public Task<IActionResult> CreateHistoricalIndexes(string indexNames, CancellationToken ct)
        {
            return RecreateOntologyIndexes(indexNames, true, false, ct);
        }

        [HttpGet("RecreateElasticOntologyIndexes/{indexNames}")]
        public Task<IActionResult> RecreateElasticOntologyIndexes(string indexNames, CancellationToken ct)
        {
            return RecreateOntologyIndexes(indexNames, false, false, ct);
        }

        [HttpGet("ReInitializeOntologyIndexes/{indexNames}")]
        public Task<IActionResult> ReInitializeOntologyIndexes(string indexNames, CancellationToken ct)
        {
            return RecreateOntologyIndexes(indexNames, false, true, ct);
        }

        [HttpGet("ReInitializeHistoricalOntologyIndexes/{indexNames}")]
        public Task<IActionResult> ReInitializeHistoricalOntologyIndexes(string indexNames, CancellationToken ct)
        {
            return RecreateOntologyIndexes(indexNames, true, true, ct);
        }

        [HttpGet("ReCreateSignIndexes/{indexNames}")]
        public Task<IActionResult> ReCreateSignIndexes(string indexNames, CancellationToken ct)
        {
            return CreateOntologyIndexes(indexNames, _elasticState.SignIndexes, false, false, ct);
        }

        [HttpGet("ReInitializeSignIndexes/{indexNames}")]
        public Task<IActionResult> ReInitializeSignIndexes(string indexNames, CancellationToken ct)
        {
            return CreateOntologyIndexes(indexNames, _elasticState.SignIndexes, true, false, ct);
        }

        private async Task<IActionResult> RecreateOntologyIndexes(string indexNames, bool isHistorical, bool useNodesFromMemory, CancellationToken ct)
        {
            var stopwatch = Stopwatch.StartNew();
            _adminElasticService.Logger = new StringBuilder();

            IEnumerable<string> indexes;
            if (indexNames == AllIndexes)
            {
                indexes = _elasticState.OntologyIndexes;
            }
            else
            {
                indexes = indexNames.Split(",");

                if (!_adminElasticService.IsIndexesValid(indexes))
                    return Content(_adminElasticService.Logger.ToString());
            }

            await _adminElasticService.DeleteIndexesAsync(indexes, isHistorical, ct);

            await _adminElasticService.CreateIndexWithMappingsAsync(indexes, isHistorical, ct);

            if (useNodesFromMemory)
                await _adminElasticService.FillIndexesFromMemoryAsync(indexes, isHistorical, ct);
            else
                await _adminElasticService.FillIndexesAsync(indexes, isHistorical, ct);

            _adminElasticService.Logger.AppendLine($"spend: {stopwatch.ElapsedMilliseconds} ms");

            return Content(_adminElasticService.Logger.ToString());
        }

        [HttpGet("RecreateElasticReportIndex")]
        public async Task<IActionResult> RecreateReportIndex(CancellationToken ct) 
        {
            _adminElasticService.Logger = new StringBuilder();
            var index = _elasticState.ReportIndex;

            await _adminElasticService.DeleteIndexesAsync(new string[] { index }, ct);

            await _adminElasticService.FillReportIndexAsync(ct);

            return Content(_adminElasticService.Logger.ToString());
        }

        [HttpGet("RecreateElasticMaterialIndexes")]
        public async Task<IActionResult> RecreateElasticMaterialIndexes(CancellationToken cancellationToken)
        {
            var log = new StringBuilder();
            var materialIndex = _elasticState.MaterialIndexes.First();

            await _elasticManager.DeleteIndexAsync(materialIndex, cancellationToken);

            var mappingConfiguration = new ElasticMappingConfiguration(new List<ElasticMappingProperty> {
                ElasticMappingProperty.CreateTextProperty("Content", ElasticConfiguration.DefaultTermVector),
                ElasticMappingProperty.CreateKeywordProperty("Metadata.features.PhoneNumber", false),
                ElasticMappingProperty.CreateDateTimeProperty("Metadata.RegTime", formats:ElasticConfiguration.DefaultDateFormats),
                ElasticMappingProperty.CreateDateTimeProperty("CreatedDate", ElasticConfiguration.DefaultDateFormats),
                ElasticMappingProperty.CreateDateTimeProperty("LoadData.ReceivingDate", ElasticConfiguration.DefaultDateFormats),
                ElasticMappingProperty.CreateKeywordProperty("ParentId", true),
                ElasticMappingProperty.CreateDenseVectorProperty("ImageVector", MaterialDocument.ImageVectorDimensionsCount)
            });

            await _elasticManager.CreateIndexesAsync(new[] { materialIndex },
                mappingConfiguration.ToJObject(),
                cancellationToken);

            var response = await _materialService.PutAllMaterialsToElasticSearchAsync(cancellationToken);

            LogElasticResult(log, response);
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

        private async Task<IActionResult> CreateOntologyIndexes(string indexNames, IEnumerable<string> baseIndexList, bool useMemoryCache, bool isHistorical, CancellationToken ct)
        {
            var stopwatch = Stopwatch.StartNew();

            var indexes = indexNames == AllIndexes ? baseIndexList : baseIndexList.Where(indexName => indexNames.Split(",").Contains(indexName, StringComparer.OrdinalIgnoreCase)).ToList();

            if(!indexes.Any()) return Content("There is no valid index names were provided.");
            
            _adminElasticService.Logger  = new StringBuilder();

            await _adminElasticService.DeleteIndexesAsync(indexes, isHistorical, ct);

            await _adminElasticService.CreateIndexWithMappingsAsync(indexes, isHistorical, ct);

            if(useMemoryCache)
                await _adminElasticService.FillIndexesFromMemoryAsync(indexes, isHistorical, ct);
            else
                await _adminElasticService.FillIndexesAsync(indexes, isHistorical, ct);

            _adminElasticService.Logger.AppendLine($"spend: {stopwatch.ElapsedMilliseconds} ms");

            return Content(_adminElasticService.Logger.ToString()); 
        }
    }
}
