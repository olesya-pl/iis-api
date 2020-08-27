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
            RunTimeSettings runTimeSettings)
        {
            _elasticManager = elasticManager;
            _ontologySchema = ontologySchema;
            _runTimeSettings = runTimeSettings;
            _elasticConfiguration = elasticConfiguration;
            _nodeRepository = nodeRepository;
            _materialRepository = materialRepository;

            var objectOfStudyType = _ontologySchema.GetEntityTypeByName(EntityTypeNames.ObjectOfStudy.ToString());
            if (objectOfStudyType != null)
            {
                OntologyIndexes = objectOfStudyType.GetAllDescendants()
                    .Where(nt => !nt.IsAbstract)
                    .Select(nt => nt.Name)
                    .ToList();

                UseElastic = true;
            }

            EventIndexes = new[]{
                "Event"
            };

            MaterialIndexes = _materialRepository.MaterialIndexes;

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

        public async Task<SearchResult> SearchByConfiguredFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default)
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
            return new SearchResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult })
            };
        }

        public async Task<SearchResult> SearchEntitiesByConfiguredFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default)
        {
            if (!UseElastic)
            {
                throw new Exception(ELASTIC_IS_NOT_USING_MSG);
            }
            var searchFields = _elasticConfiguration.GetOntologyIncludedFields(typeNames.Where(p => OntologyIndexes.Contains(p))).ToList();
            var searchByHistoryParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames.Select(GetHistoricalIndex).ToList(),
                Query = string.IsNullOrEmpty(filter.Suggestion) ? "*" : $"{filter.Suggestion}",
                From = filter.Offset,
                Size = filter.Limit,
                SearchFields = searchFields,
                ResultFields = new List<string> { "Id" }
            };

            var searchByHistoryResult = await _elasticManager.Search(searchByHistoryParams, cancellationToken);
            var entityIds = searchByHistoryResult.Items.Select(x => x.SearchResult["Id"].Value<string>()).Distinct();
            var highlightsById = searchByHistoryResult.Items.ToDictionary(k => k.SearchResult["Id"].Value<string>(), v => v.Higlight);

            var multiSearchParams = new MultiElasticSearchParams 
            {
                BaseIndexNames = typeNames.ToList(),
                From = filter.Offset,
                Size = filter.Limit,
                SearchParams = new List<(string Query, List<IIisElasticField> Fields)> 
                {
                    (string.IsNullOrEmpty(filter.Suggestion) ? "*" : $"{filter.Suggestion}", searchFields),
                    (string.Join(" ", entityIds), new List<IIisElasticField>
                    {
                        new IisElasticField
                        {
                            Name = "Id",
                            Boost = 0.5m
                        }
                    })
                }
               
            };

            /*
            {
                "militaryRank": [
                "<em>чмо</em> <em>болотне</em>"
                ]
            }

            {
              "Id": [
                "<em>dcdce839defa4cbd92d506210291f299</em>"
              ]
            }
             */

            var searchResult = await _elasticManager.Search(multiSearchParams, cancellationToken);
            return new SearchResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items.ToDictionary(
                    k => new Guid(k.Identifier),
                    v => new SearchResultItem { Highlight = CombineHighlights(highlightsById[v.Identifier], v.Higlight, v.Identifier), SearchResult = v.SearchResult })
            };
        }

        private JToken CombineHighlights(JToken historicalHighlights, JToken actualHighlights, string entityId) 
        {
            if (historicalHighlights == null)
                return actualHighlights;

            var result = (JObject)historicalHighlights.DeepClone();
            foreach (var item in ((JObject)actualHighlights).Children<JProperty>())
            {
                if (item.Name == "Id" && item.Value.Contains(entityId))
                    continue;

                
                result.Add(item);
            }

            return result;

        }

        private string GetHistoricalIndex(string typeName) 
        {
            return $"historical_{typeName}";
        }

        public Task<SearchResult> SearchMaterialsByConfiguredFieldsAsync(IElasticNodeFilter filter, CancellationToken cancellationToken = default)
        {
            if (!UseElastic)
            {
                throw new Exception(ELASTIC_IS_NOT_USING_MSG);
            }

            return _materialRepository.SearchMaterials(filter, cancellationToken);
        }

        public async Task<SearchResult> SearchMoreLikeThisAsync(IElasticNodeFilter filter, CancellationToken cancellationToken = default)
        {
            var searchParameters = new IisElasticSearchParams
            {
                BaseIndexNames = MaterialIndexes.ToList(),
                Query = filter.Suggestion,
                From = filter.Offset,
                Size = filter.Limit
            };

            var searchResult = await _elasticManager.SearchMoreLikeThisAsync(searchParameters, cancellationToken);       

            return new SearchResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult })
            };
        }

        public Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!_runTimeSettings.PutSavedToElastic || !UseElastic) return Task.FromResult(true);

            return _nodeRepository.PutNodeAsync(id, cancellationToken);
        }

        public Task<bool> PutHistoricalNodesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!_runTimeSettings.PutSavedToElastic || !UseElastic) return Task.FromResult(true);

            return _nodeRepository.PutHistoricalNodesAsync(id, cancellationToken);
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

        public async Task<SearchResult> SearchByImageVector(decimal[] imageVector, int offset, int size, CancellationToken token)
        {
            var searchResult = await _elasticManager.SearchByImageVector(imageVector, new IisElasticSearchParams {
                BaseIndexNames = MaterialIndexes.ToList(),
                From = offset,
                Size = size
            }, token);
            return new SearchResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult })
            };
        }

        private bool OntologyIndexIsSupported(string indexName)
        {
            return OntologyIndexes.Any(index => index.Equals(indexName)) || EventIndexes.Any(index => index.Equals(indexName));
        }

        private bool OntologyIndexesAreSupported(IEnumerable<string> indexNames)
        {
            if (!UseElastic) return false;
            return indexNames.All(indexName => OntologyIndexIsSupported(indexName));
        }
    }
}
