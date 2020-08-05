using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Iis.Api;
using Iis.Domain.Elastic;
using Iis.DataModel;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.DbLayer.Repositories;

namespace IIS.Core.Ontology.EntityFramework
{
    public class ElasticService : IElasticService
    {
        private readonly IElasticManager _elasticManager;
        private readonly IElasticConfiguration _elasticConfiguration;
        private readonly IOntologySchema _ontologySchema;
        private readonly RunTimeSettings _runTimeSettings;
        private readonly OntologyContext _context;
        private readonly INodeRepository _nodeRepository;
        private readonly IMaterialRepository _materialRepository;
        private const string ELASTIC_IS_NOT_USING_MSG = "Elastic is not using in current configuration";

        public IEnumerable<string> MaterialIndexes { get; }
        public IEnumerable<string> OntologyIndexes { get; }
        public IEnumerable<string> EventIndexes { get; }
        public IEnumerable<string> FeatureIndexes { get; }

        public bool UseElastic { get; private set; }

        public ElasticService(
            IElasticManager elasticManager,
            IElasticConfiguration elasticConfiguration,
            IOntologySchema ontologySchema,
            INodeRepository nodeRepository,
            IMaterialRepository materialRepository,
            RunTimeSettings runTimeSettings,
            OntologyContext context)
        {
            _elasticManager = elasticManager;
            _ontologySchema = ontologySchema;
            _runTimeSettings = runTimeSettings;
            _elasticConfiguration = elasticConfiguration;
            _context = context;
            _nodeRepository = nodeRepository;
            _materialRepository = materialRepository;

            var objectOfStudyType = _ontologySchema.GetEntityTypeByName(EntityTypeNames.ObjectOfStudy.ToString());
            if (objectOfStudyType != null)
            {
                OntologyIndexes = objectOfStudyType.GetAllDescendants()
                    .Where(nt => !nt.IsAbstract)
                    .Select(nt => nt.Name)
                    .ToList();
            }

            EventIndexes = new[]{
                "Event"
            };

            UseElastic = _context.NodeTypes.Any(nt => nt.Name == EntityTypeNames.ObjectOfStudy.ToString());

            MaterialIndexes = new[] { "Materials" };

            FeatureIndexes = new[] { "Features" };
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
            return (searchResult.Items.Select(p => new Guid(p.Identifier)).ToList(), searchResult.Count);
        }

        public async Task<SearchByConfiguredFieldsResult> SearchByConfiguredFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default)
        {
            if (!UseElastic)
            {
                throw new Exception(ELASTIC_IS_NOT_USING_MSG);
            }

            var searchFields = new List<IisElasticField>();
            var ontologyFields
                = _elasticConfiguration.GetOntologyIncludedFields(typeNames.Where(p => OntologyIndexes.Contains(p)));
            var materialFields = _elasticConfiguration.GetMaterialsIncludedFields(typeNames.Where(p => MaterialIndexes.Contains(p)));

            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                Query = string.IsNullOrEmpty(filter.Suggestion) ? "*" : $"{filter.Suggestion}",
                From = filter.Offset,
                Size = filter.Limit,
                SearchFields = ontologyFields.Union(materialFields).ToList()
            };
            var searchResult = await _elasticManager.Search(searchParams, cancellationToken);
            return new SearchByConfiguredFieldsResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchByConfiguredFieldsResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult })
            };
        }

        public Task<SearchByConfiguredFieldsResult> SearchMaterialsByConfiguredFieldsAsync(IElasticNodeFilter filter, CancellationToken cancellationToken = default)
        {
            if (!UseElastic)
            {
                throw new Exception(ELASTIC_IS_NOT_USING_MSG);
            }

            return _materialRepository.SearchMaterials(filter, cancellationToken);
        }

        public Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!_runTimeSettings.PutSavedToElastic || !UseElastic) return Task.FromResult(true);

            return _nodeRepository.PutNodeAsync(id, cancellationToken);
        }

        public async Task<bool> PutFeatureAsync(Guid featureId, JObject featureDocument, CancellationToken cancellation = default)
        {
            if (!UseElastic) return true;

            if (!_runTimeSettings.PutSavedToElastic) return false;

            if (featureDocument is null) return false;

            return await _elasticManager.PutDocumentAsync(FeatureIndexes.FirstOrDefault(), featureId.ToString("N"), featureDocument.ToString(Formatting.None));
        }
        public bool TypesAreSupported(IEnumerable<string> typeNames)
        {
            return OntologyIndexesAreSupported(typeNames);
        }

        private bool OntologyIndexIsSupported(string indexName)
        {
            return OntologyIndexes.Any(index => index.Equals(indexName)) || EventIndexes.Any(index => indexName.Equals(indexName));
        }

        private bool OntologyIndexesAreSupported(IEnumerable<string> indexNames)
        {
            if (!UseElastic) return false;
            return indexNames.All(indexName => OntologyIndexIsSupported(indexName));
        }
    }
}
