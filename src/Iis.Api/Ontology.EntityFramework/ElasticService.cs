using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Iis.Api;
using Iis.Domain;
using Iis.Domain.Elastic;
using Iis.DataModel;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Materials;

namespace IIS.Core.Ontology.EntityFramework
{
    public class ElasticService : IElasticService
    {
        private IElasticManager _elasticManager;
        private IElasticSerializer _elasticSerializer;
        private IExtNodeService _extNodeService;
        private RunTimeSettings _runTimeSettings;
        private readonly OntologyContext _context;
        private const string ELASTIC_IS_NOT_USING_MSG = "Elastic is not using in current configuration";

        public IEnumerable<string> MaterialIndexes { get; }
        public IEnumerable<string> OntologyIndexes { get; }
        public bool UseElastic { get; private set; }

        public ElasticService(IElasticManager elasticManager, IElasticSerializer elasticSerializer, IExtNodeService extNodeService, RunTimeSettings runTimeSettings, OntologyContext context)
        {
            _elasticManager = elasticManager;
            _elasticSerializer = elasticSerializer;
            _extNodeService = extNodeService;
            _runTimeSettings = runTimeSettings;
            _context = context;

            OntologyIndexes = new [] {
                "Organization",
                "Person",
                "ObjectOfStudy",
                "Radionetwork",
                "MilitaryMachinery",
                "Unknown",
                "MilitaryBase",
                "Infrastructure",
                "Subdivision",
                "SecondarySpecialEducationalInstitution",
                "HigherEducationalInstitution",
                "EducationalInstitution",
                "MilitaryOrganization"
            };

            UseElastic = _context.NodeTypes.Any(nt => nt.Name == "ObjectOfStudy");

            MaterialIndexes = new[] { "Materials" };
        }

        public async Task<(List<Guid> ids, int count)> SearchByAllFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default)
        {
            if (!UseElastic)
            {
                throw new Exception(ELASTIC_IS_NOT_USING_MSG);
            }

            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                Query = $"*{filter.Suggestion}*",
                From = filter.Offset,
                Size = filter.Limit
            };
            var searchResult = await _elasticManager.Search(searchParams, cancellationToken);
            return (searchResult.Identifiers.Select(id => new Guid(id)).ToList(), searchResult.Count);
        }

        public async Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!_runTimeSettings.PutSavedToElastic || !UseElastic) return true;

            var extNode = await _extNodeService.GetExtNodeByIdAsync(id, cancellationToken);

            return await PutNodeAsync(extNode, cancellationToken);
        }

        public async Task<bool> PutNodeAsync(IExtNode extNode, CancellationToken cancellationToken = default)
        {
            if (!UseElastic) return true;

            var json = _elasticSerializer.GetJsonByExtNode(extNode);
            
            return await _elasticManager.PutDocumentAsync(extNode.NodeTypeName, extNode.Id, json, cancellationToken);
        }

        public async Task<bool> PutMaterialAsync(IMaterialEntity material, List<IMLResponseEntity> mLResponses, CancellationToken cancellation = default)
        {
            if (!UseElastic) return true;

            if (!_runTimeSettings.PutSavedToElastic) return false;

            var jDocument = new JObject(
                new JProperty(nameof(material.Source).ToLower(), material.Source),
                new JProperty(nameof(material.Type).ToLower(), material.Type)
            );

            if (!string.IsNullOrWhiteSpace(material.Title))
            {
                jDocument.Add(nameof(material.Title), material.Title);
            }

            if (!string.IsNullOrWhiteSpace(material.Data))
            {
                JArray
                .Parse(material.Data)
                .Select(token => new JProperty(token.Value<string>("Type"), token.Value<string>("Text")))
                .Select(property =>
                {
                    jDocument.Add(property);
                    return property;
                })
                .ToList();
            }

            if (!string.IsNullOrWhiteSpace(material.LoadData))
            {
                jDocument.Merge(JObject.Parse(material.LoadData));
            }


            return await _elasticManager.PutDocumentAsync(MaterialIndexes.FirstOrDefault(), material.Id.ToString("N"), jDocument.ToString(Formatting.None));
        }

        public bool TypesAreSupported(IEnumerable<string> typeNames)
        {
            return OntologyIndexesAreSupported(typeNames);
        }

        private bool OntologyIndexIsSupported(string indexName)
        {
            return OntologyIndexes.Any(index => index.Equals(indexName));
        }

        private bool OntologyIndexesAreSupported(IEnumerable<string> indexNames)
        {
            if (!UseElastic) return false;
            return indexNames.All(indexName => OntologyIndexIsSupported(indexName));
        }

        private async Task UpdateElasticAsync(Guid nodeTypeId, string indexName, CancellationToken cancellationToken = default)
        {
            ElasticCompareResult compareResult = await CompareWithElasticAsync(nodeTypeId, indexName, cancellationToken);

            foreach (Guid id in compareResult.NeedToDelete)
            {
                await _elasticManager.DeleteDocumentAsync(indexName, id.ToString("N"));
            }

            foreach (Guid id in compareResult.NeedToUpdate)
            {
                await PutNodeAsync(id, cancellationToken);
            }
        }

        private async Task<ElasticCompareResult> CompareWithElasticAsync(Guid nodeTypeId, string indexName, CancellationToken cancellationToken = default)
        {
            IEnumerable<string> list = await _elasticManager.GetDocumentIdListFromIndexAsync(indexName);
            var elasticIds = list.Select(x => Guid.ParseExact(x, "N"));

            var query =
                from n in _context.Nodes
                where !n.IsArchived && n.NodeTypeId == nodeTypeId
                select n.Id;
            List<Guid> dbIds = await query.ToListAsync(cancellationToken);

            var toDelete = elasticIds.ToHashSet();
            toDelete.ExceptWith(dbIds);

            return new ElasticCompareResult
            {
                NeedToDelete = toDelete.ToList(),
                NeedToUpdate = dbIds
            };
        }

        private Task<string> GetNodeByIdAsync(string indexName, string id, IEnumerable<NodeType> nodeTypes)
        {
            return _elasticManager.GetDocumentByIdAsync(indexName, id, nodeTypes.Select(nt => nt.Name).ToArray());
        }
    }
}
