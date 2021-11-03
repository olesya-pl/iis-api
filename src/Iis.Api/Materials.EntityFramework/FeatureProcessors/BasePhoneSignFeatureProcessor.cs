using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials.FeatureProcessors;
using Iis.Interfaces.Constants;
using Iis.Interfaces.Ontology.Schema;

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
        protected readonly IOntologySchema _ontologySchema;
        protected readonly IElasticState _elasticState;
        protected readonly MutationCreateResolver _createResolver;
        protected readonly MutationUpdateResolver _updateResolver;
        protected readonly ILocationHistoryService _locationHistoryService;
        protected abstract string SignTypeName { get; }
        protected abstract IReadOnlyCollection<string> PrioritizedFields { get; }
        protected abstract Dictionary<string, string> SignFieldsMapping { get; }
        protected virtual string LatitudeFeaturePropertyName => SignFields.Latitude;
        protected virtual string LongitudeFeaturePropertyName => SignFields.Longitude;

        public virtual bool IsDummy => false;

        protected BasePhoneSignFeatureProcessor(IElasticService elasticService,
            IOntologySchema ontologySchema,
            MutationCreateResolver createResolver,
            MutationUpdateResolver updateResolver,
            IElasticState elasticState,
            ILocationHistoryService locationHistoryService)
        {
            _elasticService = elasticService;
            _ontologySchema = ontologySchema;
            _createResolver = createResolver;
            _updateResolver = updateResolver;
            _elasticState = elasticState;
            _locationHistoryService = locationHistoryService;
        }
        public virtual async Task<JObject> ProcessMetadataAsync(ProcessingMaterialEntry entry)
        {
            if (!FeaturesSectionExists(entry.Metadata)) return entry.Metadata;

            var signType = _ontologySchema.GetEntityTypeByName(SignTypeName);

            var features = entry.Metadata.SelectToken(FeatureFields.FeaturesSection);

            foreach (JObject originalFeature in features)
            {
                RemoveFeatureEmptyProperties(originalFeature);

                var feature = NormalizeFeatureProperties(originalFeature);

                if (!FeatureCouldBeProcessed(feature)) continue;

                var searchResult = await SearchExistingFeatureAsync(feature);

                if (searchResult.isExist)
                {
                    originalFeature[FeatureFields.featureId] = searchResult.featureId.ToString();

                    var updatesResult = ShouldExistingBeUpdated(searchResult.feature, feature);

                    if (updatesResult.shouldBeUpdate)
                    {
                        var properties = GetPropertiesFromFeature(updatesResult.updates);

                        var entity = await _updateResolver.UpdateEntityAsync(signType, searchResult.featureId.Value, properties);

                        originalFeature[FeatureFields.featureId] = entity.Id.ToString();

                        if (searchResult.featureId.Value != entity.Id)
                        {
                            var propertiesToAdd = GetPropertiesFromFeature(searchResult.feature)
                                .Where(pair => !properties.ContainsKey(pair.Key))
                                .ToDictionary(pair => pair.Key, pair => pair.Value);

                            await _updateResolver.UpdateEntityAsync(signType, entity.Id, propertiesToAdd);
                        }
                    }
                }
                else
                {
                    var properties = GetPropertiesFromFeature(feature);

                    var entity = await _createResolver.CreateEntity(Guid.NewGuid(), signType, string.Empty, properties, Guid.NewGuid());

                    originalFeature[FeatureFields.featureId] = entity.Id.ToString();
                }
            }

            var locationHistoryCollection = await GetLocationHistoryCollectionAsync(entry);

            await SaveLocationHistoryCollectionAsync(locationHistoryCollection);

            await UpldateSignElasticDocumentAsync(entry);

            return entry.Metadata;
        }

        public IEnumerable<Guid> GetValidFeatureIds(IEnumerable<Guid> featureIdList)
        {
            return featureIdList;
        }

        protected virtual bool FeaturesSectionExists(JObject metadata) =>
            metadata.ContainsKey(FeatureFields.FeaturesSection) &&
            metadata.SelectToken(FeatureFields.FeaturesSection) is JArray &&
            metadata.SelectToken(FeatureFields.FeaturesSection).HasValues;

        protected virtual bool FeatureCouldBeProcessed(JObject feature)
        {
            if (!feature.HasValues) return false;

            var properties = feature.Properties().Select(p => p.Name);

            return PrioritizedFields.Intersect(properties).Any();
        }

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

                if (property is null) continue;

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
                var fieldValue = feature.GetValue(field, StringComparison.OrdinalIgnoreCase)?.Value<string>();

                if (string.IsNullOrWhiteSpace(fieldValue)) continue;

                var searchResult = await SearchFeatureInElasticSearchAsync(field, fieldValue);

                if (searchResult.isExist) return (searchResult.isExist, field, searchResult.featureId, searchResult.feature);
            }
            return (false, null, null, null);
        }

        protected virtual async Task<(bool isExist, Guid? featureId, JObject feature)> SearchFeatureInElasticSearchAsync(string fieldName, string fieldValue)
        {
            var filter = new ElasticFilter { Limit = 1, Offset = 0, Suggestion = $"({fieldName}:\"{fieldValue}\")" };

            var searchResult = await _elasticService.SearchSignsAsync(SignTypeIndexNames, filter);

            if (searchResult.Count == 0) return (false, null, null);

            var featurePair = searchResult.Items.FirstOrDefault();

            featurePair.Value.SearchResult.Remove(HIGHLIGHT);

            var feature = featurePair.Value.SearchResult.Value<JObject>();

            return (true, featurePair.Key, feature);
        }

        protected virtual Task<IReadOnlyCollection<LocationHistoryDto>> GetLocationHistoryCollectionAsync(ProcessingMaterialEntry entry)
        {
            var result = new List<LocationHistoryDto>();

            var features = entry.Metadata.SelectToken(FeatureFields.FeaturesSection);

            var locationTimeStamp = entry.RegistrationDate ?? entry.CreatedDate;

            foreach (JObject feature in features)
            {
                var coordinatesResult = TryFetchCoordinatiesFromFeature(feature);

                var entityIdResult  = TryFetchEntityIdFromFeature(feature);

                if (!coordinatesResult.IsSuccess || !entityIdResult.IsSuccess) continue;

                result.Add(new LocationHistoryDto
                {
                    EntityId = entityIdResult.FeatureId,
                    NodeId = entityIdResult.FeatureId,
                    Lat = coordinatesResult.Latitude,
                    Long = coordinatesResult.Longitude,
                    RegisteredAt = locationTimeStamp,
                    MaterialId = entry.Id
                });
            }

            return Task.FromResult<IReadOnlyCollection<LocationHistoryDto>>(result);
        }

        private async Task UpldateSignElasticDocumentAsync(ProcessingMaterialEntry entry)
        {
            var features = entry.Metadata.SelectToken(FeatureFields.FeaturesSection);

            foreach (JObject feature in features)
            {
                var entityIdResult  = TryFetchEntityIdFromFeature(feature);

                if (!entityIdResult.IsSuccess || entityIdResult.FeatureId == Guid.Empty) continue;

                await _elasticService.PutNodeAsync(entityIdResult.FeatureId);
            }
        }

        private Task SaveLocationHistoryCollectionAsync(IReadOnlyCollection<LocationHistoryDto> collection)
        {
            return _locationHistoryService.SaveLocationHistoryAsync(collection);
        }

        private (decimal Latitude, decimal Longitude, bool IsSuccess) TryFetchCoordinatiesFromFeature(JObject feature)
        {
            var latStringValue = feature.Property(LatitudeFeaturePropertyName)?.Value.Value<string>();
            var lonStringValue = feature.Property(LongitudeFeaturePropertyName)?.Value.Value<string>();

            var parseResult = Decimal.TryParse(latStringValue, out decimal latitude)
                           & Decimal.TryParse(lonStringValue, out decimal longitude);

            return (Latitude: latitude, Longitude: longitude, IsSuccess: parseResult);
        }

        private (Guid FeatureId, bool IsSuccess) TryFetchEntityIdFromFeature(JObject feature)
        {
            var featureIdStringValue = feature.Property(FeatureFields.featureId).Value.Value<string>();

            var parseResult = Guid.TryParse(featureIdStringValue, out Guid featureId);

            return (FeatureId: featureId, IsSuccess: parseResult);
        }
    }
}