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

namespace IIS.Core.Ontology.EntityFramework
{
    public class ElasticService : IElasticService
    {
        private IElasticManager _elasticManager;
        private IElasticSerializer _elasticSerializer;
        private IExtNodeService _extNodeService;
        private IElasticConfiguration _elasticConfiguration;
        private RunTimeSettings _runTimeSettings;
        private readonly OntologyContext _context;
        private const string ELASTIC_IS_NOT_USING_MSG = "Elastic is not using in current configuration";

        public IEnumerable<string> MaterialIndexes { get; }
        public IEnumerable<string> OntologyIndexes { get; }
        public bool UseElastic { get; private set; }

        public ElasticService(
            IElasticManager elasticManager,
            IElasticSerializer elasticSerializer,
            IExtNodeService extNodeService,
            IElasticConfiguration elasticConfiguration,
            RunTimeSettings runTimeSettings,
            OntologyContext context)
        {
            _elasticManager = elasticManager;
            _elasticSerializer = elasticSerializer;
            _extNodeService = extNodeService;
            _runTimeSettings = runTimeSettings;
            _elasticConfiguration = elasticConfiguration;
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
                "MilitaryOrganization",
                "TerrorOrganization"
            };

            UseElastic = _context.NodeTypes.Any(nt => nt.Name == "ObjectOfStudy");

            MaterialIndexes = new[] { "Materials" };
        }

        public async Task<SearchByAllFieldsResult> SearchByAllFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default)
        {
            if (!UseElastic)
            {
                throw new Exception(ELASTIC_IS_NOT_USING_MSG);
            }

            var searchFields = new List<IisElasticField>();
            var fields = _elasticConfiguration.GetIncludedFieldsByTypeNames(typeNames);

            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                Query = $"{filter.Suggestion}",
                From = filter.Offset,
                Size = filter.Limit,
                SearchFields = fields
            };
            var searchResult = await _elasticManager.Search(searchParams, cancellationToken);
            return new SearchByAllFieldsResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchByAllFieldsResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult })
            };
        }

        public async Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!_runTimeSettings.PutSavedToElastic || !UseElastic) return true;

            var extNode = await _extNodeService.GetExtNodeByIdAsync(id, cancellationToken);

            return await PutNodeAsync(extNode, cancellationToken);
        }

        public async Task<bool> PutNodeAsync(IExtNode extNode, CancellationToken cancellationToken = default)
        {
            if (!UseElastic || !OntologyIndexIsSupported(extNode.NodeTypeName)) return true;

            var json = _elasticSerializer.GetJsonByExtNode(extNode);

            return await _elasticManager.PutDocumentAsync(extNode.NodeTypeName, extNode.Id, json, cancellationToken);
        }

        public async Task<bool> PutMaterialAsync(Guid materialId, JObject materialDocument, CancellationToken cancellation = default)
        {
            if (!UseElastic) return true;

            if (!_runTimeSettings.PutSavedToElastic) return false;

            if(materialDocument is null) return false;

            return await _elasticManager.PutDocumentAsync(MaterialIndexes.FirstOrDefault(), materialId.ToString("N"), materialDocument.ToString(Formatting.None));
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
            IEnumerable<string> list = (await _elasticManager.GetDocumentIdListFromIndexAsync(indexName))
                .Items.Select(p => p.Identifier);
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
