﻿using Iis.DbLayer.Repositories;
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
                new ElasticMappingProperty("Metadata.features.PhoneNumber", ElasticMappingPropertyType.Keyword),
                new ElasticMappingProperty("CreatedDate", ElasticMappingPropertyType.Date, formats:ElasticConfiguration.DefaultDateFormats),
                new ElasticMappingProperty("LoadData.ReceivingDate", ElasticMappingPropertyType.Date),
                new ElasticMappingProperty("Data.Text", ElasticMappingPropertyType.Text),
                new ElasticMappingProperty("Children.Data.Text", ElasticMappingPropertyType.Text),
                new ElasticMappingProperty("ParentId", ElasticMappingPropertyType.Keyword, true)
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
