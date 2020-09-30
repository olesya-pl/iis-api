using Iis.DbLayer.Repositories;
using Iis.Elastic;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Schema;
using IIS.Core.Materials;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iis.OntologyData;
using MoreLinq;
using Iis.Services.Contracts.Interfaces;
using System.Diagnostics;

namespace Iis.Api.Controllers
{
    [Route("{controller}")]
    [ApiController]
    public class AdminController : Controller
    {
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
            _elasticManager = elasticManager;
            _materialService = materialService;
            _nodeRepository = nodeRepository;
            _elasticState = elasticState;
            _adminElasticService = adminElasticService;
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

        private async Task<IActionResult> RecreateOntologyIndexes(string indexNames, bool isHistorical, bool useNodesFromMemory, CancellationToken ct)
        {
            var stopwatch = Stopwatch.StartNew();
            _adminElasticService.Logger = new StringBuilder();

            IEnumerable<string> indexes;
            if (indexNames == "all")
            {
                indexes = _elasticState.OntologyIndexes;
            }
            else
            {
                indexes = indexNames.Split(",");

                if (!_adminElasticService.IsIndexesValid(indexes))
                    return Content(_adminElasticService.Logger.ToString());
            }

            await _adminElasticService.DeleteOntologyIndexesAsync(indexes, isHistorical, ct);
            await _adminElasticService.CreateOntologyMappingsAsync(indexes, isHistorical, ct);

            if (useNodesFromMemory)
                await _adminElasticService.FillOntologyIndexesFromMemoryAsync(indexes, isHistorical, ct);
            else
                await _adminElasticService.FillOntologyIndexesAsync(indexes, isHistorical, ct);

            _adminElasticService.Logger.AppendLine($"spend: {stopwatch.ElapsedMilliseconds} ms");
            return Content(_adminElasticService.Logger.ToString());
        }

        [HttpGet("RecreateElasticReportIndex")]
        public async Task<IActionResult> RecreateReportIndex(CancellationToken ct) 
        {
            var log = new StringBuilder();
            var index = _elasticState.ReportIndex;

            await _adminElasticService.DeleteIndexesAsync(new string[] { index }, ct);

            return Content(log.ToString());
        }

        [HttpGet("RecreateElasticMaterialIndexes")]
        public async Task<IActionResult> RecreateElasticMaterialIndexes(CancellationToken cancellationToken)
        {
            var log = new StringBuilder();
            var materialIndex = _elasticState.MaterialIndexes.First();

            await _elasticManager.DeleteIndexAsync(materialIndex, cancellationToken);

            var mappingConfiguration = new ElasticMappingConfiguration(new List<ElasticMappingProperty> {
                new ElasticMappingProperty("Content", ElasticMappingPropertyType.Text, termVector: ElasticConfiguration.DefaultTermVector),
                new ElasticMappingProperty("Metadata.features.PhoneNumber", ElasticMappingPropertyType.Keyword),
                new ElasticMappingProperty("Metadata.RegTime", ElasticMappingPropertyType.Date, formats:ElasticConfiguration.DefaultDateFormats),
                new ElasticMappingProperty("CreatedDate", ElasticMappingPropertyType.Date, formats:ElasticConfiguration.DefaultDateFormats),
                new ElasticMappingProperty("LoadData.ReceivingDate", ElasticMappingPropertyType.Date, formats:ElasticConfiguration.DefaultDateFormats),
                new ElasticMappingProperty("ParentId", ElasticMappingPropertyType.Keyword, true),
                new ElasticMappingProperty("ImageVector", ElasticMappingPropertyType.DenseVector, dimensions:MaterialDocument.ImageVectorDimensionsCount)
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
    }
}
