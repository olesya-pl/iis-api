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
    public abstract class BasePhoneSignFeatureProcessor : IFeatureProcessor
    {
        private const string HIGHLIGHT = "highlight";
        private string SignTypeIndexName => SignTypeName;
        private string[] SignTypeIndexNames => new[] { SignTypeIndexName };
        protected static JsonMergeSettings mergeSettings = new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union,
            PropertyNameComparison = StringComparison.OrdinalIgnoreCase,
            MergeNullValueHandling = MergeNullValueHandling.Ignore
        };
        protected readonly IElasticService _elasticService;
        protected readonly IOntologyModel _ontology;
        protected readonly IElasticState _elasticState;
        protected readonly MutationCreateResolver _createResolver;
        protected readonly MutationUpdateResolver _updateResolver;
        protected abstract string SignTypeName { get; }
        protected abstract List<string> PrioritizedFields { get; }
        protected abstract Dictionary<string, string> SignFieldsMapping { get; }

        public virtual bool IsDummy => false;

        public BasePhoneSignFeatureProcessor(IElasticService elasticService,
            IOntologyModel ontology,
            MutationCreateResolver createResolver,
            MutationUpdateResolver updateResolver,
            IElasticState elasticState)
        {
            _elasticService = elasticService;
            _ontology = ontology;
            _createResolver = createResolver;
            _updateResolver = updateResolver;
            _elasticState = elasticState;
        }
        public virtual async Task<JObject> ProcessMetadataAsync(JObject metadata)
        {
            if (!FeaturesSectionExists(metadata)) return metadata;

            var signType = _ontology.GetEntityType(SignTypeName);

            var features = metadata.SelectToken(FeatureFields.FeaturesSection);

            foreach (JObject originalFeature in features)
            {
                RemoveFeatureEmptyProperties(originalFeature);

                var feature = NormalizeFeatureProperties(originalFeature);

                if (!feature.HasValues) continue;

                var searchResult = await SearchExistingFeatureAsync(feature);

                if (searchResult.isExist)
                {
                    originalFeature[FeatureFields.featureId] = searchResult.featureId.ToString();

                    var updatesResult = ShouldExistingBeUpdated(searchResult.feature, feature);

                    if (!updatesResult.shouldBeUpdate) continue;

                    var updatedFeature = MergeFeatures(searchResult.feature, updatesResult.updates);

                    var properties = GetPropertiesFromFeature(updatesResult.updates);

                    var entity = await _updateResolver.UpdateEntity(signType, searchResult.featureId.Value, properties);

                    await _elasticService.PutFeatureAsync(SignTypeIndexName, searchResult.featureId.Value, updatedFeature);
                }
                else
                {
                    var properties = GetPropertiesFromFeature(feature);

                    var entity = await _createResolver.CreateEntity(signType, properties);

                    originalFeature[FeatureFields.featureId] = entity.Id.ToString();

                    await _elasticService.PutFeatureAsync(SignTypeIndexName, entity.Id, feature);
                }
            }
            return metadata;
        }
        protected virtual bool FeaturesSectionExists(JObject metadata) =>
            metadata.ContainsKey(FeatureFields.FeaturesSection) &&
            metadata.SelectToken(FeatureFields.FeaturesSection) is JArray &&
            metadata.SelectToken(FeatureFields.FeaturesSection).HasValues;
        protected virtual JObject RemoveFeatureEmptyProperties(JObject feature)
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
        protected virtual JObject NormalizeFeatureProperties(JObject feature)
        {
            var result = new JObject();

            foreach (var (featureFieldName, signFieldName) in SignFieldsMapping)
            {
                var property = feature.Property(featureFieldName);
                
                if(property is null) continue;

                if (featureFieldName == FeatureFields.featureId) continue;

                var propertyValue = property.Value.Value<string>();

                if (string.IsNullOrWhiteSpace(propertyValue)) continue;

                result.Add(signFieldName, property.Value);
            }
            return result;
        }
        protected virtual Dictionary<string, object> GetPropertiesFromFeature(JObject feature)
        {
            var properties = new Dictionary<string, object>();

            foreach (var (featureFieldName, signFieldName) in SignFieldsMapping)
            {
                var fieldValue = feature.GetValue(signFieldName, StringComparison.OrdinalIgnoreCase)?.Value<string>();

                if (string.IsNullOrWhiteSpace(fieldValue)) continue;

                properties.Add(signFieldName, fieldValue);
            }

            return properties;
        }
        protected virtual JObject MergeFeatures(JObject existingFeature, JObject newFeature)
        {
            existingFeature.Merge(newFeature, mergeSettings);

            return existingFeature;
        }
        protected virtual (bool shouldBeUpdate, JObject updates) ShouldExistingBeUpdated(JObject existingFeature, JObject newFeature)
        {
            var updates = new JObject();

            var propertyNames = existingFeature.Children().Union(newFeature.Children()).Select(_ => (_ as JProperty).Name).Distinct();

            foreach (var propertyName in propertyNames)
            {
                var newProperty = newFeature[propertyName];

                var existingProperty = existingFeature[propertyName];

                if (newProperty is null) continue;

                if (existingProperty is null)
                {
                    updates.Add(propertyName, newProperty.Value<JValue>());

                    continue;
                }

                if (!JToken.DeepEquals(existingProperty, newProperty))
                {
                    updates.Add(propertyName, newProperty.Value<JValue>());
                }
            }
            return (updates.HasValues, updates);
        }
        protected virtual async Task<(bool isExist, string fieldName, Guid? featureId, JObject feature)> SearchExistingFeatureAsync(JObject feature)
        {
            foreach (var field in PrioritizedFields)
            {
                var fieldName = field.ToLowerInvariant();

                var fieldValue = feature.GetValue(fieldName, StringComparison.OrdinalIgnoreCase)?.Value<string>();

                if (string.IsNullOrWhiteSpace(fieldValue)) continue;

                var searchResult = await SearchFeatureInElasticSearchAsync(fieldName, fieldValue);

                if (searchResult.isExist) return (searchResult.isExist, fieldName, searchResult.featureId, searchResult.feature);
            }
            return (false, null, null, null);
        }
        protected virtual async Task<(bool isExist, Guid? featureId, JObject feature)> SearchFeatureInElasticSearchAsync(string fieldName, string fieldValue)
        {
            var searchResult = await _elasticService.SearchByConfiguredFieldsAsync(
                        SignTypeIndexNames,
                        new ElasticFilter { Limit = 1, Offset = 0, Suggestion = $"({fieldName}:{fieldValue})" });

            if (searchResult.Count == 0) return (false, null, null);

            var featurePair = searchResult.Items.FirstOrDefault();

            featurePair.Value.SearchResult.Remove(HIGHLIGHT);

            var feature = featurePair.Value.SearchResult.Value<JObject>();

            return (true, featurePair.Key, feature);
        }

    }
}