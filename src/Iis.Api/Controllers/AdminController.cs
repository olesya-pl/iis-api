using Iis.DbLayer.Repositories;
using Iis.Elastic;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
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
        IMaterialRepository _materialRepository;
        IMaterialService _materialService;
        public AdminController(
            IExtNodeService extNodeService,
            IMaterialService materialService,
            IElasticService elasticService,
            IElasticManager elasticManager,
            IOntologySchema ontologySchema,
            INodeRepository nodeRepository,
            IMaterialRepository materialRepository)
        {
            _extNodeService = extNodeService;
            _elasticManager = elasticManager;
            _elasticService = elasticService;
            _materialService = materialService;
            _ontologySchema = ontologySchema;
            _nodeRepository = nodeRepository;
            _materialRepository = materialRepository;
        }

        [HttpPost("CreateHistoricalIndexes")]
        public async Task<IActionResult> CreateHistoricalIndexes(CancellationToken ct) 
        {
            //var ontologyIndexes = _elasticService.OntologyIndexes;
            var ontologyIndexes = new List<string> {"Person" };
            var historicalIndexesByTypeName = ontologyIndexes.ToDictionary(x => x, x => $"historical_{x}");

            await _elasticManager.DeleteIndexesAsync(historicalIndexesByTypeName.Values, ct);

            foreach (var index in ontologyIndexes)
            {
                var type = _ontologySchema.GetEntityTypeByName(index);
                var attributesInfo = _ontologySchema.GetAttributesInfo(index);
                
                var properties = attributesInfo.Items.ToList();
                properties.Add(new AttributeInfoItem("actualDatePeriod", ScalarType.DateRange, null));
                
                var updateAttributesInfo = new AttributeInfo(historicalIndexesByTypeName[index], properties.Select(x => (AttributeInfoItem)x));

                var result = await _elasticManager.CreateMapping(updateAttributesInfo);
            }

            var extNodes = await _extNodeService.GetExtNodesByTypeIdsAsync(ontologyIndexes, ct);
            foreach (var extNode in extNodes)
            {
                await _elasticService.PutHistoricalNodesAsync(extNode, null, ct);
            }

            return Ok();
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

                var notValidNames = ontologyIndexes
                    .Where(name => !_elasticService.OntologyIndexes.Contains(name))
                    .ToList();

                if (notValidNames.Count > 0)
                {
                    sb.AppendLine("There are not valid index names in list:");
                    notValidNames.ForEach(name => sb.AppendLine(name));
                    return Content(sb.ToString());
                }
            }

            await _elasticManager.DeleteIndexesAsync(ontologyIndexes, cancellationToken);

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
    }
}
