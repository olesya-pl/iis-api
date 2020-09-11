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

namespace Iis.Api.Controllers
{
    [Route("{controller}")]
    [ApiController]
    public class AdminController : Controller
    {
        IExtNodeService _extNodeService;
        IElasticManager _elasticManager;
        IElasticService _elasticService;
        IOntologySchema _ontologySchema;
        INodeRepository _nodeRepository;
        IMaterialService _materialService;
        public AdminController(
            IExtNodeService extNodeService,
            IMaterialService materialService,
            IElasticService elasticService,
            IElasticManager elasticManager,
            IOntologySchema ontologySchema,
            INodeRepository nodeRepository)
        {
            _extNodeService = extNodeService;
            _elasticManager = elasticManager;
            _elasticService = elasticService;
            _materialService = materialService;
            _ontologySchema = ontologySchema;
            _nodeRepository = nodeRepository;
        }

        [HttpPost("CreateHistoricalIndexes/{indexNames}")]
        public async Task<IActionResult> CreateHistoricalIndexes(string indexNames,CancellationToken ct) 
        {
            IEnumerable<string> ontologyIndexes;
            IDictionary<string, string> historicalIndexesByTypeName;
            var log = new StringBuilder();
            if (indexNames == "all") 
            {
                ontologyIndexes = _elasticService.OntologyIndexes;
                historicalIndexesByTypeName = _elasticService.HistoricalOntologyIndexes;
            }
            else 
            {
                ontologyIndexes = indexNames.Split(",");
                historicalIndexesByTypeName = _elasticService.HistoricalOntologyIndexes
                    .Where(x => ontologyIndexes.Contains(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);

                if(!IsIndexesValid(ontologyIndexes, log))
                    return Content(log.ToString());
            }

            await _elasticManager.DeleteIndexesAsync(historicalIndexesByTypeName.Values, ct);

            foreach (var index in ontologyIndexes)
            {
                var attributesInfo = _ontologySchema.GetHistoricalAttributesInfo(index, historicalIndexesByTypeName[index]);
                await _elasticManager.CreateMapping(attributesInfo);
            }

            var extNodes = await _extNodeService.GetExtNodesByTypeIdsAsync(ontologyIndexes, ct);
            foreach (var extNode in extNodes)
            {
                await _elasticService.PutHistoricalNodesAsync(extNode, null, ct);
            }

            log.AppendLine($"{extNodes.Count} nodes added");
            return Content(log.ToString());
        }


        [HttpGet("RecreateElasticOntologyIndexes/{indexNames}")]
        public async Task<IActionResult> RecreateElasticOntologyIndexes(string indexNames, CancellationToken cancellationToken)
        {
            IEnumerable<string> ontologyIndexes;
            var log = new StringBuilder();
            if (indexNames == "all")
            {
                ontologyIndexes = _elasticService.OntologyIndexes.ToList();
            }
            else
            {
                ontologyIndexes = indexNames.Split(',');

                if (!IsIndexesValid(ontologyIndexes, log))
                    return Content(log.ToString());
            }

            await _elasticManager.DeleteIndexesAsync(ontologyIndexes, cancellationToken);

            foreach (var ontologyIndex in ontologyIndexes)
            {
                var attributesInfo = _ontologySchema.GetAttributesInfo(ontologyIndex);
                await _elasticManager.CreateMapping(attributesInfo);
            }

            var extNodes = await _extNodeService.GetExtNodesByTypeIdsAsync(ontologyIndexes, cancellationToken);
            var result = await _nodeRepository.PutNodesAsync(extNodes, cancellationToken);

            log.AppendLine($"nodes were found: {extNodes.Count}");
            LogElasticResult(log, result);
            return Content(log.ToString());
        }

        private void LogElasticResult(StringBuilder log, IEnumerable<ElasticBulkResponse> response) 
        {
            var responseBySuccess = response.GroupBy(x => new { x.IsSuccess, x.SuccessOperation });

            foreach (var item in responseBySuccess.Where(x => x.Key.IsSuccess)) 
            {
                log.AppendLine($"{item.Key.SuccessOperation}: {item.Count()}");
            }

            foreach (var item in responseBySuccess.Where(x => !x.Key.IsSuccess))
            {
                foreach (var error in item)
                {
                    log.AppendLine($"error occurred for Id:{error.Id}, errorType:{error.ErrorType}, error message:{error.ErrorReason}");
                }
                
            }
        }


        [HttpGet("RecreateElasticMaterialIndexes/{indexNames}")]
        public async Task<IActionResult> RecreateElasticMaterialIndexes(CancellationToken cancellationToken)
        {
            var materialIndex = _elasticService.MaterialIndexes.First();

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

            var materialsCount = await _materialService.PutAllMaterialsToElasticSearchAsync(cancellationToken);

            return Content($"{materialsCount} materials completed");
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

        private bool IsIndexesValid(IEnumerable<string> indexes, StringBuilder log)
        {
            var notValidNames = indexes
                       .Where(name => !_elasticService.OntologyIndexes.Contains(name))
                       .ToList();

            if (notValidNames.Count > 0)
            {
                log.AppendLine("There are not valid index names in list:");
                notValidNames.ForEach(name => log.AppendLine(name));
                return false;
            }

            return true;
        }
    }
}
