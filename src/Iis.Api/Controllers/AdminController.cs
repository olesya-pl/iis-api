﻿using Iis.Elastic;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.Repository;
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
        IMaterialProvider _materialProvider;
        IOntologySchema _ontologySchema;
        INodeRepository _nodeRepository;
        public AdminController(
            IExtNodeService extNodeService,
            IMaterialProvider materialProvider,
            IElasticService elasticService,
            IElasticManager elasticManager,
            IOntologySchema ontologySchema,
            INodeRepository nodeRepository)
        {
            _extNodeService = extNodeService;
            _elasticManager = elasticManager;
            _elasticService = elasticService;
            _materialProvider = materialProvider;
            _ontologySchema = ontologySchema;
            _nodeRepository = nodeRepository;
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
            var materialEntities = await _materialProvider.GetMaterialEntitiesAsync();

            var materialIndex = _elasticService.MaterialIndexes.First();

            await _elasticManager.DeleteIndexAsync(materialIndex, cancellationToken);

            var mappingConfiguration = new ElasticMappingConfiguration(new List<ElasticMappingProperty> {
                new ElasticMappingProperty("Metadata.features.PhoneNumber", ElasticMappingPropertyType.Keyword),
                new ElasticMappingProperty("createddate", ElasticMappingPropertyType.Date),
                new ElasticMappingProperty("LoadData.ReceivingDate", ElasticMappingPropertyType.Date)
            });

            await _elasticManager.CreateIndexesAsync(new[] { materialIndex },
                mappingConfiguration.ToJObject(),
                cancellationToken);

            var entityTasks = materialEntities
                                .Select(async entity =>
                                {
                                    var document = await _materialProvider.GetMaterialDocumentAsync(entity.Id);
                                    return await _elasticService.PutMaterialAsync(entity.Id, document, cancellationToken);
                                });

            await Task.WhenAll(entityTasks);
            return Content($"{materialEntities.Count()} materials completed");
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
