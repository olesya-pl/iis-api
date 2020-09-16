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
        private readonly OntologyNodesData ontologyNodesData;
        IMaterialService _materialService;
        public AdminController(
            IExtNodeService extNodeService,
            IMaterialService materialService,
            IElasticService elasticService,
            IElasticManager elasticManager,
            IOntologySchema ontologySchema,
            INodeRepository nodeRepository,
            OntologyNodesData ontologyNodesData)
        {
            _extNodeService = extNodeService;
            _elasticManager = elasticManager;
            _elasticService = elasticService;
            _materialService = materialService;
            _ontologySchema = ontologySchema;
            _nodeRepository = nodeRepository;
            this.ontologyNodesData = ontologyNodesData;
        }

        [HttpPost("CreateHistoricalIndexes/{indexNames}")]
        public async Task<IActionResult> CreateHistoricalIndexes(string indexNames, CancellationToken ct)
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

                if (!IsIndexesValid(ontologyIndexes, log))
                    return Content(log.ToString());
            }

            await _elasticManager.DeleteIndexesAsync(historicalIndexesByTypeName.Values, ct);

            foreach (var index in ontologyIndexes)
            {
                var attributesInfo = _ontologySchema.GetHistoricalAttributesInfo(index, historicalIndexesByTypeName[index]);
                await _elasticManager.CreateMapping(attributesInfo);
            }

            var extNodes = await _extNodeService.GetExtNodesByTypeIdsAsync(ontologyIndexes, ct);
            var response = await _nodeRepository.PutHistoricalNodesAsync(extNodes, ct);

            LogElasticResult(log, response);
            return Content(log.ToString());
        }

        [HttpGet("RecreateElasticOntologyIndexes/{indexNames}")]
        public async Task<IActionResult> RecreateElasticOntologyIndexes(string indexNames, CancellationToken cancellationToken)
        {
            IEnumerable<string> ontologyIndexes;
            var sb = new StringBuilder();
            if (indexNames == "all")
            {
                ontologyIndexes = _elasticService.OntologyIndexes.ToList();
            }
            else
            {
                ontologyIndexes = indexNames.Split(',');

                if (!IsIndexesValid(ontologyIndexes, sb))
                    return Content(sb.ToString());
            }

            var deleteTasks = new List<Task>();
            foreach (var index in ontologyIndexes)
            {
                deleteTasks.Add(_elasticManager.DeleteIndexAsync(index, cancellationToken));
            }
            await Task.WhenAll(deleteTasks);

            foreach (var ontologyIndex in ontologyIndexes)
            {
                var type = _ontologySchema.GetEntityTypeByName(ontologyIndex);
                var attributesInfo = _ontologySchema.GetAttributesInfo(ontologyIndex);
                await _elasticManager.CreateMapping(attributesInfo);
            }

            var extNodes = await _extNodeService.GetExtNodesByTypeIdsAsync(ontologyIndexes, cancellationToken);
            foreach (var extNode in extNodes)
            {
                await _elasticService.PutNodeAsync(extNode, cancellationToken);
            }

            sb.AppendLine($"{extNodes.Count} nodes added");
            return Content(sb.ToString());
        }

        [HttpGet("ReInitializeOntologyIndexes/{indexNames}")]
        public async Task<IActionResult> ReInitializeOntologyIndexes(string indexNames, CancellationToken cancellationToken)
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
            var deleteTasks = new List<Task>();
            foreach (var index in ontologyIndexes)
            {
                deleteTasks.Add(_elasticManager.DeleteIndexAsync(index, cancellationToken));
            }
            await Task.WhenAll(deleteTasks);

            var itemsToUpdate = new List<Interfaces.Ontology.Data.INode>();
            foreach (var ontologyIndex in ontologyIndexes)
            {
                var type = _ontologySchema.GetEntityTypeByName(ontologyIndex);
                var attributesInfo = _ontologySchema.GetAttributesInfo(ontologyIndex);
                await _elasticManager.CreateMapping(attributesInfo);
                var entities = ontologyNodesData.GetEntitiesByTypeName(type.Name);
                itemsToUpdate.AddRange(entities);
            }

            var response = await _nodeRepository.PutNodesAsync(itemsToUpdate, cancellationToken);
            LogElasticResult(log, response);

            return Content(log.ToString());
        }

        [HttpGet("ReInitializeHistoricalOntologyIndexes/{indexNames}")]
        public async Task<IActionResult> ReInitializeHistoricalOntologyIndexes(string indexNames, CancellationToken ct)
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

                if (!IsIndexesValid(ontologyIndexes, log))
                    return Content(log.ToString());
            }

            foreach (var item in historicalIndexesByTypeName.Values)
            {
                await _elasticManager.DeleteIndexAsync(item, ct);
            }

            foreach (var index in ontologyIndexes)
            {
                var attributesInfo = _ontologySchema.GetHistoricalAttributesInfo(index, historicalIndexesByTypeName[index]);
                await _elasticManager.CreateMapping(attributesInfo);
            }

            var nodes = new List<Interfaces.Ontology.Data.INode>();
            foreach (var index in ontologyIndexes)
            {
                var type = _ontologySchema.GetEntityTypeByName(index);
                var entities = ontologyNodesData.GetEntitiesByTypeName(type.Name);
                nodes.AddRange(entities);
            }

            var response = await _nodeRepository.PutHistoricalNodesAsync(nodes, ct);

            LogElasticResult(log, response);
            return Content(log.ToString());
        }

        [HttpGet("RecreateElasticMaterialIndexes/{indexNames}")]
        public async Task<IActionResult> RecreateElasticMaterialIndexes(CancellationToken cancellationToken)
        {
            var log = new StringBuilder();
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
