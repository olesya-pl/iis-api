using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials.FeatureProcessors;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class GSMFeatureProcessor : IFeatureProcessor
    {
        private const string HIGHLIGHT = "highlight";
        private const string SignTypeName = "CellphoneSign";
        private readonly IElasticService _elasticService;
        private readonly IOntologyModel _ontology;
        private readonly IElasticState _elasticState;
        private readonly MutationCreateResolver _createResolver;
        private readonly MutationUpdateResolver _updateResolver;

        private static readonly List<string> prioritizedFields = new List<string>
        {
            FeatureFields.IMSI,
            FeatureFields.PhoneNumber,
            FeatureFields.IMEI,
            FeatureFields.DBObject
        };
        private static readonly Dictionary<string, string> signTypeFields = new Dictionary<string, string>
        {
            { PhoneSignFields.PhoneNumber, FeatureFields.PhoneNumber },
            { PhoneSignFields.DBObject, FeatureFields.DBObject },
            { PhoneSignFields.IMEI, FeatureFields.IMEI },
            { PhoneSignFields.IMSI, FeatureFields.IMSI },
            { PhoneSignFields.TMSI, FeatureFields.TMSI }
        };
        private static readonly JsonMergeSettings mergeSettings = new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union,
            PropertyNameComparison = StringComparison.OrdinalIgnoreCase,
            MergeNullValueHandling = MergeNullValueHandling.Ignore
        };

        public bool IsDummy => false;
        public GSMFeatureProcessor(IElasticService elasticService,
            IOntologyModel ontology,
            MutationCreateResolver createResolver,
            MutationUpdateResolver updateResolver, IElasticState elasticState)
        {
            _elasticService = elasticService;
            _ontology = ontology;
            _createResolver = createResolver;
            _updateResolver = updateResolver;
            _elasticState = elasticState;
        }

        public async Task<JObject> ProcessMetadataAsync(JObject metadata)
        {
            if (!FeaturesSectionExists(metadata)) return metadata;

            var signType = _ontology.GetEntityType(SignTypeName);

            var features = metadata.SelectToken(FeatureFields.FeaturesSection);

            foreach (JObject originalFeature in features)
            {
                RemoveEmptyProperties(originalFeature);

                var feature = NormalizeObject(originalFeature);

                if(!feature.HasValues) continue;

                var searchResult = await SearchExistingFeature(feature);

                if (searchResult.isExist)
                {
                    originalFeature[FeatureFields.featureId] = searchResult.featureId.ToString();

                    var updatesResult = DoesExistingShouldBeUpdated(searchResult.feature, feature);

                    if(!updatesResult.shouldBeUpdate) continue;

                    var updatedFeature = MergeFeatures(updatesResult.updates, searchResult.feature);

                    await PutFeatureToElasticSearch(searchResult.featureId.Value, updatedFeature);

                    var properties = GetPropertiesForEntity(updatesResult.updates);

                    var entity = await _updateResolver.UpdateEntity(signType, searchResult.featureId.Value, properties);
                }
                else
                {
                    var properties = GetPropertiesForEntity(feature);

                    var entity = await _createResolver.CreateEntity(signType, properties);

                    originalFeature[FeatureFields.featureId] = entity.Id.ToString();

                    await PutFeatureToElasticSearch(entity.Id, feature);
                }
            }

            return metadata;
        }

        private bool FeaturesSectionExists(JObject metadata) =>
            metadata.ContainsKey(FeatureFields.FeaturesSection) &&
            metadata.SelectToken(FeatureFields.FeaturesSection) is JArray &&
            metadata.SelectToken(FeatureFields.FeaturesSection).HasValues;
        
        private async Task<(bool isExist, string fieldName, Guid? featureId, JObject feature)> SearchExistingFeature(JObject feature)
        {
            foreach (var field in prioritizedFields)
            {
                var fieldName = field.ToLowerInvariant();

                var fieldValue = feature.GetValue(fieldName, StringComparison.OrdinalIgnoreCase)?.Value<string>();

                if (string.IsNullOrWhiteSpace(fieldValue)) continue;

                var searchResult = await SearchFeatureInElasticSearch(fieldName, fieldValue);

                if (searchResult.isExist) return (searchResult.isExist, fieldName, searchResult.featureId, searchResult.feature);
            }
            return (false, null, null, null);
        }

        private async Task<(bool isExist, Guid? featureId, JObject feature)> SearchFeatureInElasticSearch(string fieldName, string fieldValue)
        {
            var searchResult = await _elasticService.SearchByConfiguredFieldsAsync(
                        _elasticState.FeatureIndexes,
                        new ElasticFilter { Limit = 1, Offset = 0, Suggestion = $"({fieldName}:{fieldValue})" });

            if (searchResult.Count == 0) return (false, null, null);

            var featurePair = searchResult.Items.FirstOrDefault();

            featurePair.Value.SearchResult.Remove(HIGHLIGHT);

            var feature = featurePair.Value.SearchResult.Value<JObject>();

            return (true, featurePair.Key, feature);
        }

        private async Task<bool> PutFeatureToElasticSearch(Guid featureId, JObject feature)
        {
            return await _elasticService.PutFeatureAsync(featureId, feature);
        }

        private Dictionary<string, object> GetPropertiesForEntity(JObject feature)
        {
            var properties = new Dictionary<string, object>();

            foreach (var (signFieldName, featureFieldName) in signTypeFields)
            {
                var fieldValue = feature.GetValue(featureFieldName, StringComparison.OrdinalIgnoreCase)?.Value<string>();

                if(string.IsNullOrWhiteSpace(fieldValue)) continue;

                properties.Add(signFieldName, fieldValue);
            }

            return properties;
        }

        private JObject MergeFeatures(JObject newFeature, JObject existingFeature)
        {
            existingFeature.Merge(newFeature, mergeSettings);

            return existingFeature;
        }

        private (bool shouldBeUpdate, JObject updates) DoesExistingShouldBeUpdated(JObject existingFeature, JObject newFeature)
        {
            var updates = new JObject();

            var propertyNames = existingFeature.Children().Union(newFeature.Children()).Select(_ => (_ as JProperty).Name).Distinct();

            foreach(var propertyName in propertyNames)
            {
                var newProperty = newFeature[propertyName];

                var existingProperty = existingFeature[propertyName];

                if(newProperty is null) continue;

                if(existingProperty is null)
                {
                    updates.Add(propertyName, newProperty.Value<JValue>());

                    continue;
                }

                if(!JToken.DeepEquals(existingProperty, newProperty))
                {
                    updates.Add(propertyName, newProperty.Value<JValue>());
                }
            }
            return (updates.HasValues, updates);
        }

        private JObject NormalizeObject(JObject feature)
        {
            var result = new JObject();

            foreach (var property in feature.Properties())
            {
                var propertyName = property.Name.ToLowerInvariant();

                var propertyValue = property.Value.Value<string>();

                if (propertyName == FeatureFields.featureId.ToLowerInvariant()) continue;

                if(string.IsNullOrWhiteSpace(propertyValue)) continue;

                result.Add(propertyName, property.Value);
            }

            return result;
        }
        private JObject RemoveEmptyProperties(JObject feature)
        {
            var emptyPropsList = new List<string>();

            foreach (var property in feature.Properties())
            {
                var propertyName = property.Name;

                var propertyValue = property.Value.Value<string>();

                if (string.IsNullOrWhiteSpace(propertyValue))
                {
                    emptyPropsList.Add(propertyName);
                }
            }

            foreach (var propertyName in emptyPropsList)
            {
                feature.Remove(propertyName);
            }

            return feature;
        }

    }

}